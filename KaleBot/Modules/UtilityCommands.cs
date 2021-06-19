using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using KaleBot.Services;
using KaleBot.Utilities;
using System;

namespace KaleBot.Modules
{
    public class UtilityCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<UtilityCommands> _logger;
        private readonly Servers _servers;
        private readonly RanksHelper _ranksHelper;

        public UtilityCommands(ILogger<UtilityCommands> logger, Servers servers, RanksHelper ranksHelper)
        {
            _logger = logger;
            _ranksHelper = ranksHelper;
            _servers = servers;
        }

        [Command("help")]
        public async Task Help(string query = "")
        {
            var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "?";
            if (string.IsNullOrEmpty(query))
            {
                var _builder = new EmbedBuilder()
                    .WithTitle("KaleBot Help")
                    .WithDescription($"For more information about a command, use {guildPrefix}help + command")
                    .AddField("Available Commands",
                    $"> ping\n" +
                    $"> prefix\n" +
                    $"> info\n" +
                    $"> server\n" +
                    $"> yt\n" +
                    $"> yts\n" +
                    $"> snipe\n", true)
                    .WithColor(Color.DarkBlue)
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithFooter(Context.Guild.Name.ToString());
                var _embed = _builder.Build();
                await ReplyAsync(null, false, _embed);
                return;
            }

            string info = "";
            string addInfo = "";
            bool commandValid = true;

            switch (query)
            {
                case "ping":
                    info = "Returns the estimated round-trip to the gateway server in ms.";
                    break;
                case "prefix":
                    info = "Get or set the bot prefix for this server";
                    addInfo = $", {guildPrefix}prefix !";
                    break;
                case "info":
                    info = "Gets info of user mentioned. Defaults to sender profile.";
                    addInfo = $", {guildPrefix}info {Context.Client.CurrentUser.Mention}";
                    break;
                case "server":
                    info = "Gets server info.";
                    break;
                case "yt":
                    info = "Get first youtube result of `keyword`.";
                    addInfo = " dog";
                    break;
                case "yts":
                    info = "Gets the first five results of `keyword`.";
                    addInfo = " dogs";
                    break;
                case "snipe":
                    info = "Gets the latest deleted message.";
                    break;
                default:
                    info = $"\"{query}\" is not a command.";
                    commandValid = false;
                    break;
            }

            var builder = new EmbedBuilder()
                .WithTitle(query)
                .WithDescription($"> {info}")
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp()
                .WithFooter(Context.Guild.Name.ToString());
            if (commandValid) builder.AddField("Usage", $"`{guildPrefix}{query}{addInfo}`");
            var embed = builder.Build();
            await ReplyAsync(null, false, embed);
        }

        [Command("ping")]
        [Alias("p")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong! " + Context.Client.Latency + "ms");
            _logger.LogInformation($"{Context.User.Username} executed the ping command!");
        }

        [Command("math")]
        public async Task MathAsync([Remainder] string math)
        {
            var dt = new DataTable();
            var result = dt.Compute(math, null);

            await ReplyAsync($"Result: {result}");
            _logger.LogInformation($"{Context.User.Username} executed the math command!");
        }

        
        /// <summary>
        /// Gets info of user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>If specified, returns info of tagged user</returns>
        [Command("info")]
        [Alias("i")]
        public async Task Info(SocketGuildUser user = null)
        {
            if (user == null)
            {
                var builder = new EmbedBuilder()
                    .WithTitle($"User info for {(Context.User as SocketGuildUser).Nickname}")
                    .WithThumbnailUrl(Context.User.GetAvatarUrl())
                    .WithColor(Color.DarkBlue)
                    .AddField($"User", Context.User, true)
                    .AddField($"Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                    .AddField($"Created at", Context.User.CreatedAt.ToString("MM/dd/yyyy"))
                    .AddField($"Joined at", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("MM/dd/yyyy"), true)
                    .WithCurrentTimestamp()
                    .WithFooter(Context.Guild.Name.ToString());

                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithTitle($"User info for {user.Nickname}")
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithColor(Color.DarkBlue)
                    .AddField($"User", user.Username, true)
                    .AddField($"Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
                    .AddField($"Created at", user.CreatedAt.ToString("MM/dd/yyyy"))
                    .AddField($"Joined at", user.JoinedAt.Value.ToString("MM/dd/yyyy"), true)
                    .WithCurrentTimestamp()
                    .WithFooter(Context.Guild.Name.ToString());

                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
            }
        }

        [Command("server")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("In this message you can find some nice information about the current server.")
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(Color.DarkBlue)
                .AddField("Created at", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"), true)
                .AddField("Member Count", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online users", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true)
                .AddField("Text Channels", Context.Guild.TextChannels.Count, true)
                .AddField("Voice Channels", Context.Guild.VoiceChannels.Count, true)
                .AddField("Roles", Context.Guild.Roles.Count, true)
                .WithCurrentTimestamp()
                .WithFooter(Context.Guild.Name.ToString());
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
        [Command("invite")]
        public async Task Invite()
        {
            var invites = await Context.Guild.GetInvitesAsync();

            await ReplyAsync(invites.Select(x => x.Url).FirstOrDefault());
        }

        [Command("links")]
        public async Task Links()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Interested in our bot? Here are some links!")
                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl())
                .AddField("Links", $"[Github](https://github.com/kaleabelgas/KaleBot)\n" +
                $"[Discord](https://discord.gg/JkF6BJEAeC)", true)
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp()
                .WithFooter(Context.Guild.Name.ToString());
            var embed = builder.Build();
            await ReplyAsync(null, false, embed);
        }

        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder]string identifier)
        {
            await Context.Channel.TriggerTypingAsync();

            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            IRole role;

            if(ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if(roleById == null)
                {
                    await ReplyAsync("Role does not exist!");
                    return;
                }
                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if(roleByName == null)
                {
                    await ReplyAsync("Role does not exist!");
                    return;
                }
                role = roleByName;
            }

            if(ranks.All(x => x.Id != role.Id))
            {
                await ReplyAsync("That rank does not exist!");
                return;
            }

            if((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await ReplyAsync($"Successfully removed rank {role.Mention} from you.");
                return;
            }
            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            await ReplyAsync($"Successfully added rank {role.Mention} to you.");
        }
    }
}
