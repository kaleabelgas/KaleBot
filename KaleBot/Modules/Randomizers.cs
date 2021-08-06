using System.Data;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using KaleBot.Services;
using System;

namespace KaleBot.Modules
{
    public class Randomizers : ModuleBase<SocketCommandContext>
    {
        [Command("coinflip")]
        public async Task CoinFlip()
        {
            var random = new Random();

            var answer = random.Next() % 2 == 0 ? "heads" : "tails";

            await ReplyAsync(answer);
        }

        [Command("die")]
        public async Task Die()
        {
            await Context.Channel.TriggerTypingAsync();
            var random = new Random();
            await ReplyAsync(random.Next(1, 7).ToString());

        }

        [Command("dice")]
        public async Task Dice()
        {
            await Context.Channel.TriggerTypingAsync();
            var random = new Random();
            await ReplyAsync(random.Next(1, 13).ToString());
        }

        [Command("choose")]
        public async Task Choose(params string[] args)
        {
            await Context.Channel.TriggerTypingAsync();
            var random = new Random();
            var word = random.Next(args.Length);

            var builder = new EmbedBuilder()
                .WithTitle("I choose...")
                .WithDescription(args[word] + "!")
                .WithCurrentTimestamp();
            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }
    }
}
