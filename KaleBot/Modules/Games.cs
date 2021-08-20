using System.Data;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using KaleBot.Services;
using System;
using System.Linq;
using System.Reflection;

namespace KaleBot.Modules
{
    public class Games : ModuleBase<SocketCommandContext>
    {

        [Command("tictactoe")]
        public async Task Tictactoe(SocketUser user)
        {
            //Tictactoe tictactoe = new Tictactoe(Context.Message.Author, user);
            //Context.Client.MessageReceived += tictactoe.ReveiveInput;
            await Task.CompletedTask;
        }

        [Command("top")]
        public async Task GetTopUsers()
        {
            var sortedEconomy = CommandHandler.Economy.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, pair => pair.Value);


            int i = 1;
            string list = "";

            foreach (var key in sortedEconomy.Keys)
            {
                var user = Context.Guild.GetUser(key);
                if (user == null) continue;
                list += $"{i}. {user.Mention}: ${sortedEconomy[key]}\n";
                i++;
            }

            var builder = new EmbedBuilder()
                .WithTitle("Top richest in this Server")
                .WithDescription(list)
                .WithCurrentTimestamp()
                .WithColor(Color.DarkBlue)
                .WithThumbnailUrl(Context.Guild.IconUrl);

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

        [Command("bal")]
        public async Task Rank()
        {
            var user = Context.Message.Author.Id;
            if (!CommandHandler.Economy.ContainsKey(user))
            {
                await ReplyAsync("You are not ranked yet!");
                return;
            }
            int myrank = 1;
            foreach (var users in CommandHandler.Economy.OrderByDescending(key => key.Value))
            {
                if (Context.Message.Author.Id == users.Key)
                {

                    var builder = new EmbedBuilder()
                        .WithTitle($"{Context.Message.Author.Username}'s Balance")
                        .WithCurrentTimestamp()
                        .WithColor(Color.DarkBlue)
                        .WithDescription($"Rank: {myrank}\n Balance: ${users.Value}")
                        .WithThumbnailUrl(Context.Message.Author.GetAvatarUrl() ?? Context.Message.Author.GetDefaultAvatarUrl());

                    var embed = builder.Build();

                    await ReplyAsync(null, false, embed);


                    return;
                }

                myrank++;
                continue;
            }

        }
    }

}
