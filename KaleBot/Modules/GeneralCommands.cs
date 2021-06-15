using System.Data;
using System.Threading.Tasks;
using Discord.Commands;
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
    }
}
