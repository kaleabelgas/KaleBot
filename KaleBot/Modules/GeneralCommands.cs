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
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis;
using System;

namespace KaleBot.Modules
{
    public class GeneralCommands : ModuleBase<SocketCommandContext>
    {
        private static Dictionary<SocketGuildUser, SocketMessage> harassList = new Dictionary<SocketGuildUser, SocketMessage>();

        [Command("say")]
        public async Task Say(string text, int amount = 1)
        {
            if (amount < 1)
            {
                await ReplyAsync("Tryna break me, are ya?");
            }
            else if (amount > 10)
            {
                await ReplyAsync("smh");
            }
            else if (Context.Message.MentionedUsers.Count > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    await ReplyAsync(Context.User.Mention + " tried to ping smh");
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
        [Command("appreciate")]
        public async Task Appreciate(SocketGuildUser user)
        {
            if(user.Mention == Context.Client.CurrentUser.Mention)
            {
                await ReplyAsync("Aww, thanks! Just doing my best.");
                return;
            }
            await ReplyAsync($"We appreciate you {user.Mention}! ❤️");
        }

        [Command("Harass")]
        public async Task StartHarass(SocketGuildUser user, [Remainder]SocketMessage message)
        {
            harassList.Add(user, message);
            Context.Client.MessageReceived += Harass;
        }

        public async Task Harass(SocketMessage message)
        {
            await ReplyAsync($"{harassList[message.Author as SocketGuildUser]}");
        }

        [Command("yts")]
        public async Task YouTubeList([Remainder] string query = "")
        {
            if (string.IsNullOrEmpty(query))
            {
                await ReplyAsync("Gotta put in at least something");
                return;
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyA9MwwtfSa5oXUrPfdoKK85V05-zlRn7qo",
                ApplicationName = this.GetType().ToString()

            });

            var searchRequest = youtubeService.Search.List("snippet");

            searchRequest.Q = query;

            searchRequest.MaxResults = 5;
            var searchResponse = await searchRequest.ExecuteAsync();

            List<string> responses = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                responses.Add($"{i + 1}. {searchResponse.Items[i].Snippet.Title} \n <https://www.youtube.com/watch?v={searchResponse.Items[i].Id.VideoId}>");
            }
            string allResponses = string.Join("\n", responses.ToArray());

            await ReplyAsync(allResponses.ToString());
        }

        [Command("yt")]
        public async Task YouTube([Remainder] string query = "")
        {
            if (string.IsNullOrEmpty(query))
            {
                await ReplyAsync("Gotta put in at least something");
                return;
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyA9MwwtfSa5oXUrPfdoKK85V05-zlRn7qo",
                ApplicationName = this.GetType().ToString()

            });

            var searchRequest = youtubeService.Search.List("snippet");

            searchRequest.Q = query;

            searchRequest.MaxResults = 11;
            var searchResponse = await searchRequest.ExecuteAsync();

            await ReplyAsync($"{searchResponse.Items[0].Snippet.Title} \n https://www.youtube.com/watch?v={searchResponse.Items[0].Id.VideoId}");
        }


        [Command("snipe")]
        public async Task Snipe()
        {
            if (CommandHandler.LastMessage == null)
            {
                await ReplyAsync("There's nothing to snipe!");
                return;
            }
            await ReplyAsync($"Deleted Message by {CommandHandler.LastMessage.Author.Mention}: {CommandHandler.LastMessage.Content}");
        }

        [Command("kill")]
        public async Task Kill([Remainder]SocketGuildUser user)
        {
            if(user == null)
            {
                await ReplyAsync("You have to tag someone, idot!");
                return;
            }

            await ReplyAsync($"{Context.User.Mention} just killed {user.Mention}, so sad.");
        }

        [Command("thx")]
        public async Task Thx([Remainder]string args = "")
        {
            if (string.IsNullOrEmpty(args))
            {
                await ReplyAsync("Thx? Thx what?");
                return;
            }
            await ReplyAsync("Eh, I can't do that (yet). May run into copyright issues as well.");
        }

        [Command("mock")]
        public async Task Mock([Remainder]string msg = "")
        {
            if (string.IsNullOrEmpty(msg)) return;
            var randomizer = new Random();
            var final =
                msg.Select(x => randomizer.Next() % 2 == 0 ?
                (char.IsUpper(x) ? x.ToString().ToLower().First() : x.ToString().ToUpper().First()) : x);
            var randomUpperLower = new string(final.ToArray());
            await ReplyAsync(randomUpperLower);
        }

    }
}
