using Discord.Commands;
using KaleBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Infrastructure;

namespace KaleBot.Modules
{
    public class Configuration : ModuleBase<SocketCommandContext>
    {
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly RanksHelper _ranksHelper;
        private readonly AutoRoles _autoRoles;
        private readonly AutoRolesHelper _autoRolesHelper;

        public Configuration(RanksHelper ranksHelper, Servers servers, Ranks ranks, AutoRoles autoRoles, AutoRolesHelper autoRolesHelper)
        {
            _ranks = ranks;
            _ranksHelper = ranksHelper;
            _autoRoles = autoRoles;
            _autoRolesHelper = autoRolesHelper;
            _servers = servers;
        }

        [Command("ranks", RunMode = RunMode.Async)]
        public async Task Ranks()
        {
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            if (ranks.Count == 0)
            {
                await ReplyAsync("This server does not have any ranks. \nIn order to add a rank, you can use the name or the ID of the role.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string description = "This message lists all available ranks.";

            foreach (var rank in ranks)
            {
                description += $"\n{rank.Mention} {(rank.Id)}";
            }

            await ReplyAsync(description);
        }

        [Command("addrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();

            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position than the bot.");
                return;
            }
            if (ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("Role already a rank.");
                return;
            }
            await _ranks.AddRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the ranks.");
        }

        [Command("delrank")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _ranksHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }
            if (ranks.All(x => x.Id == role.Id))
            {
                await ReplyAsync("This role is not a rank!");
                return;
            }
            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"THe role {role.Mention} has been removed from the ranks.");
        }




        /// <summary>
        /// Lists all AutoRoles.
        /// </summary>
        /// <returns></returns>
        [Command("autoroles", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AutoRoles()
        {
            var autoroles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            if (autoroles.Count == 0)
            {
                await ReplyAsync("This server does not have any autoroles!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string description = "This message lists all available ranks. \nIn order to remove an autorole, use the name or id of the role.";

            foreach (var autorole in autoroles)
            {
                description += $"\n{autorole.Mention} {(autorole.Id)}";
            }

            await ReplyAsync(description);
        }

        [Command("addautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();

            var autoRoles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }
            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position than the bot.");
                return;
            }
            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("Role already an autorole.");
                return;
            }
            await _autoRoles.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the autoroles.");
        }

        [Command("delautorole")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoroles = await _autoRolesHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }
            if (autoroles.All(x => x.Id == role.Id))
            {
                await ReplyAsync("This role is not an autorole!");
                return;
            }
            await _autoRoles.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"THe autorole {role.Mention} has been removed from the ranks.");
        }


        [Command("prefix", RunMode = RunMode.Async)]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "?";
                await ReplyAsync($"Current prefix: `{guildPrefix}`");
                return;
            }

            if (prefix.Length > 5)
            {
                await ReplyAsync($"Prefix too long! Use string <= 8 characters");
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"Prefix changed to `{prefix}`");
        }
    }
}
