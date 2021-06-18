
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KaleBot.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;

        public static SocketUserMessage LastMessage { get; private set; }

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config, Servers servers) : base(client, logger)
        {
            _provider = provider;
            _service = commandService;
            _config = config;
            _servers = servers;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.MessageDeleted += SnipeSet;
            _client.MessageDeleted += GhostPingDetect;
            _client.ChannelCreated += OnChannelCreated;
            _client.ReactionAdded += OnReactionAdded;
            _service.CommandExecuted += OnCommandExecuted;
            _client.Connected += SetActivity;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task SetActivity()
        {
            await _client.SetGameAsync($"?help; watching over {_client.Guilds.Count} servers!", null, ActivityType.Playing);

        }

        private async Task GhostPingDetect(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            if((arg1.Value as SocketUserMessage).MentionedUsers.Count < 1) { return; }

            var builder = new EmbedBuilder()
                .WithTitle("Ghost ping detected!")
                .AddField("Sender", (arg1.Value as SocketUserMessage).Author.Mention, false)
                .AddField("Message", (arg1.Value as SocketUserMessage).Content.ToString(), false)
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await (arg2 as ITextChannel).SendMessageAsync(null, false, embed);
        }

        private async Task SnipeSet(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            LastMessage = arg1.Value as SocketUserMessage;
            //await arg2.SendMessageAsync($"Deleted Message: {(arg1.Value as SocketMessage).Content}");
        }



        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.MessageId != 854262115108126720) return;
            if (arg3.Emote.Name != "<:pogfish: 828762718629658635 >") return;
            var role = (arg2 as SocketGuildChannel).Guild.GetRole(854263151720660992);
            await (arg3.User.Value as SocketGuildUser).AddRoleAsync(role);
        }

        private async Task OnChannelCreated(SocketChannel arg)
        {
            if ((arg as ITextChannel) == null) return;
            var channel = arg as ITextChannel;
            await channel.SendMessageAsync("New Channel Created! Hello!!");
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "?";
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) Console.WriteLine(result);
        }
    }
}
