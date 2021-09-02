
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using KaleBot.Modules;
using KaleBot.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KaleBot.Services
{
    public class ValuePair
    {
        public string HarassEmote;
        public ulong Harasser;
    }
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        public static List<Mute> Mutes = new List<Mute>();

        public static Dictionary<string, string> UserCommands = new Dictionary<string, string>();
        public static Dictionary<ulong, ValuePair> HarassList = new Dictionary<ulong, ValuePair>();
        public static Dictionary<ulong, int> Economy = new Dictionary<ulong, int>();

        public static int Multiplier { get; set; } = 1;

        public static SocketUserMessage LastMessage { get; private set; }
        public static SocketUserMessage LastMessageEdit { get; private set; }


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
            _client.MessageReceived += HarassManager;
            _client.MessageDeleted += SnipeSet;
            _client.MessageReceived += OnUserMessage;
            _client.MessageDeleted += GhostPingDetect;
            _client.MessageUpdated += EditDetect;
            _client.ChannelCreated += OnChannelCreated;
            _client.ReactionAdded += OnReactionAdded;

            _client.Connected += async () => await _client.SetGameAsync($"?help: watching over {_client.Guilds.Count} servers!", null, ActivityType.Playing);
            _client.Connected += SetCommandDictionary;
            _client.Connected += SetHarassDictionary;
            _client.Connected += SetEconomyDictionary;
            _client.Connected += async () =>
            {
                using (StreamReader file = File.OpenText(@"counting.json"))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    int num = (int)jsonSerializer.Deserialize(file, typeof(int));
                    GamesData.LastNumber = num;
                }
                await Task.CompletedTask;
            };

            _service.CommandExecuted += OnCommandExecuted;

            var newTask = new Task(async () => await MuteHandler());
            newTask.Start();

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnUserMessage(SocketMessage arg)
        {
            var id = arg.Author.Id;
            var path = @"economy.json";

            if (!Economy.Keys.Contains(id))
            {
                Economy.Add(id, 1);
                File.WriteAllText(path, JsonConvert.SerializeObject(Economy));
                return;
            }

            Economy[id] += 1 * Multiplier;


            File.WriteAllText(path, JsonConvert.SerializeObject(Economy));

            await Task.CompletedTask;
        }

        private async Task SetEconomyDictionary()
        {
            using (StreamReader file = File.OpenText(@"economy.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                var cc = (Dictionary<ulong, int>)serializer.Deserialize(file, typeof(Dictionary<ulong, int>));
                if (cc == null) return;
                Economy = cc;
            }
            await Task.CompletedTask;
        }

        private async Task SetCommandDictionary()
        {
            using (StreamReader file = File.OpenText(@"customcommands.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                var cc = (Dictionary<string, string>)serializer.Deserialize(file, typeof(Dictionary<string, string>));
                if (cc == null) return;
                UserCommands = cc;
            }
            await Task.CompletedTask;
        }

        private async Task SetHarassDictionary()
        {
            using (StreamReader file = File.OpenText(@"harasslist.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                var hl = (Dictionary<ulong, ValuePair>)serializer.Deserialize(file, typeof(Dictionary<ulong, ValuePair>));
                if (hl == null) return;
                HarassList = hl;
            }
            await Task.CompletedTask;
        }

        private async Task HarassManager(SocketMessage userMessage)
        {
            if (!HarassList.ContainsKey(userMessage.Author.Id))
            {
                return;
            }
            var emote = Emote.Parse(HarassList[userMessage.Author.Id].HarassEmote);
            Task.Delay(200);
            await userMessage.AddReactionAsync(emote);
            //await userMessage.Channel.SendMessageAsync(HarassList[userMessage.Author.Id]);
        }

        private async Task EditDetect(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            LastMessageEdit = arg1.Value as SocketUserMessage;
            await Task.CompletedTask;
        }

        private async Task GhostPingDetect(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            if ((arg1.Value as SocketUserMessage) == null) return;
            if ((arg1.Value as SocketUserMessage).MentionedUsers.Count < 1) { return; }

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
            await Task.CompletedTask;
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
            //bool bruh = (arg is SocketUserMessage message);
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
            if (command.IsSpecified) Console.WriteLine($"{context.Message.Author.Username} used {command.Value.Name}!\n" +
                $"Context: {context.Message.Content}");
            await Task.CompletedTask;
        }

        private async Task MuteHandler()
        {
            List<Mute> Remove = new List<Mute>();

            foreach (var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                    continue;

                var guild = _client.GetGuild(mute.Guild.Id);

                if (guild.GetRole(mute.Role.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var role = guild.GetRole(mute.Role.Id);

                if (guild.GetUser(mute.User.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id);

                if (role.Position > guild.CurrentUser.Hierarchy)
                {
                    Remove.Add(mute);
                    continue;
                }

                await user.RemoveRoleAsync(mute.Role);
                Remove.Add(mute);
            }

            Mutes = Mutes.Except(Remove).ToList();

            await Task.Delay(1 * 60 * 1000);
            await MuteHandler();
        }
    }
}
