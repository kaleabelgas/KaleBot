using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace KaleBot.Modules
{
    public class GeneralCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<GeneralCommands> _logger;
        private readonly Servers _servers;

        public GeneralCommands(ILogger<GeneralCommands> logger, Servers servers)
        {
            _logger = logger;
            _servers = servers;
        }

        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
            _logger.LogInformation($"{Context.User.Username} executed the ping command!");
        }

        [Command("echo")]
        public async Task EchoAsync([Remainder] string text)
        {
            await ReplyAsync(text);
            _logger.LogInformation($"{Context.User.Username} executed the echo command!");
        }

        [Command("math")]
        public async Task MathAsync([Remainder] string math)
        {
            var dt = new DataTable();
            var result = dt.Compute(math, null);

            await ReplyAsync($"Result: {result}");
            _logger.LogInformation($"{Context.User.Username} executed the math command!");
        }

        [Command("ssay")]
        public async Task Spam(string text, int amount = 1)
        {
            if(amount < 1)
            {
                await ReplyAsync("bruh dont break the bot");
            }
            else if(amount > 10)
            {
                await ReplyAsync("smh");
            }
            else if(Context.Message.MentionedUsers.Count > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    await ReplyAsync(Context.User.Mention + "is bad!!!");
                }
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    await ReplyAsync(text);
                }

            }
        }

        [Command("prefix")]
        [RequireUserPermission(Discord.ChannelPermission.ManageRoles)]
        public async Task Prefix(string prefix = null)
        {
            if(prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "?";
                await ReplyAsync($"Current prefix: `{guildPrefix}`");
                return;
            }

            if(prefix.Length > 5)
            {
                await ReplyAsync($"Prefix too long! Use string <= 8 characters");
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"Prefix changed to `{prefix}`");
        }
        [Command("info")]
        public async Task Info(SocketGuildUser user = null)
        {
            if(user == null)
            {
                var builder = new EmbedBuilder()
                    .WithTitle("Info")
                    .WithThumbnailUrl(Context.User.GetAvatarUrl())
                    .WithDescription($"User Info for {Context.User.Username}")
                    .AddField($"User", Context.User, true)
                    .AddField($"Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                    .AddField($"Created at", Context.User.CreatedAt.ToString("MM/dd/yyyy"))
                    .AddField($"Joined at", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("MM/dd/yyyy"), true)
                    .WithCurrentTimestamp();

                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithTitle("Info")
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .WithDescription($"User Info for {user.Username}")
                    .AddField($"User", user.Username, true)
                    .AddField($"Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
                    .AddField($"Created at", user.CreatedAt.ToString("MM/dd/yyyy"))
                    .AddField($"Joined at", user.JoinedAt.Value.ToString("MM/dd/yyyy"), true)
                    .WithCurrentTimestamp();

                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
            }
        }

        [Command("appreciate")]
        public async Task Appreciate(SocketGuildUser user)
        {
            await ReplyAsync($"We appreciate you {user.Mention}! <3");
        }

        [Command("yt")]
        public async Task YouTube(string query)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyA9MwwtfSa5oXUrPfdoKK85V05-zlRn7qo",
                ApplicationName = this.GetType().ToString()
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query;
            searchRequest.MaxResults = 1;

            var searchResponse = await searchRequest.ExecuteAsync();

            await ReplyAsync($"{searchResponse.Items[0].Snippet.Title}{searchResponse.Items[0].Id.VideoId}");
        }
    }
}
