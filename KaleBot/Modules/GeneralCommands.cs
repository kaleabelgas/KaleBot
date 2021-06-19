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
using System.Web;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace KaleBot.Modules
{
    public class GeneralCommands : ModuleBase<SocketCommandContext>
    {
        private static Dictionary<SocketGuildUser, SocketMessage> harassList = new Dictionary<SocketGuildUser, SocketMessage>();

        [Command("say")]
        public async Task Say(int amount = 1, [Remainder]string text = "")
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
        [Command("say")]
        public async Task Say([Remainder] string text = "")
        {
            int amount = 1;
            if (Context.Message.MentionedUsers.Count > 0)
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
        public async Task Appreciate(SocketGuildUser user = null)
        {
            if(user == null)
            {
                await ReplyAsync("Appreciate? Appreciate whom?");
                return;
            }
            if(user.Mention == Context.Client.CurrentUser.Mention)
            {
                await ReplyAsync("Aww, thanks! Just doing my best.");
                return;
            }
            await ReplyAsync($"We appreciate you {user.Mention}! ❤️");
        }

        //[Command("Harass")]
        //public async Task StartHarass(SocketGuildUser user, [Remainder]SocketMessage message)
        //{
        //    harassList.Add(user, message);
        //    Context.Client.MessageReceived += Harass;
        //}

        public async Task Harass(SocketMessage message)
        {
            await ReplyAsync($"{harassList[message.Author as SocketGuildUser]}");
        }

        [Command("ytl", RunMode = RunMode.Async)]
        public async Task YouTubeList([Remainder] string query = "")
        {
            if (string.IsNullOrEmpty(query))
            {
                await ReplyAsync("Gotta put in at least something");
                return;
            }
            await Context.Channel.TriggerTypingAsync();

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyA9MwwtfSa5oXUrPfdoKK85V05-zlRn7qo",
                ApplicationName = this.GetType().ToString()

            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.Strict;
            searchRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
            searchRequest.RegionCode = "US";
            searchRequest.Type = "video";

            searchRequest.Q = query;

            searchRequest.MaxResults = 5;
            var searchResponse = await searchRequest.ExecuteAsync();

            if(searchResponse.Items.Count == 0)
            {
                await ReplyAsync($"No results found for \"{query}\".");
                return;
            }

            List<string> responses = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                var title = HttpUtility.HtmlDecode(searchResponse.Items[i].Snippet.Title);

                responses.Add($"**{i + 1}. {title}** \n<https://www.youtube.com/watch?v={searchResponse.Items[i].Id.VideoId}>\n");
            }
            string allResponses = string.Join("\n", responses.ToArray());


            var builder = new EmbedBuilder()
                .WithTitle($"YouTube search results for \"{query}\"")
                .WithDescription(allResponses)
                .WithColor(Color.DarkBlue)
                .WithImageUrl(searchResponse.Items[0].Snippet.Thumbnails.High.Url)
                .WithCurrentTimestamp()
                .WithFooter(Context.Guild.Name.ToString());
            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

        [Command("yt", RunMode = RunMode.Async)]
        public async Task YouTube([Remainder] string query = "")
        {
            if (string.IsNullOrEmpty(query))
            {
                await ReplyAsync("Gotta put in at least something");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyA9MwwtfSa5oXUrPfdoKK85V05-zlRn7qo",
                ApplicationName = this.GetType().ToString()

            });

            var searchRequest = youtubeService.Search.List("snippet");

            searchRequest.Q = query;
            searchRequest.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.Strict;
            searchRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
            searchRequest.RegionCode = "US";
            searchRequest.Type = "video";

            searchRequest.MaxResults = 1;
            var searchResponse = await searchRequest.ExecuteAsync();


            if (searchResponse.Items.Count == 0)
            {
                await ReplyAsync($"No results found for \"{query}\"");
                return;
            }

            var result = HttpUtility.HtmlDecode(searchResponse.Items[0].Snippet.Title);


            var builder = new EmbedBuilder()
                .WithTitle($"YouTube search result for \"{query}\"")
                .WithColor(Color.DarkBlue)
                .WithImageUrl(searchResponse.Items[0].Snippet.Thumbnails.High.Url)
                .WithDescription($"[**{result}**](https://www.youtube.com/watch?v={searchResponse.Items[0].Id.VideoId})")
                .WithCurrentTimestamp()
                .WithFooter(Context.Guild.Name.ToString());
            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
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
        
        [Command("coinflip")]
        public async Task CoinFlip()
        {
            var random = new Random();

            var answer = random.Next() % 2  == 0 ? "heads" : "tails";

            await ReplyAsync(answer);
        }

        [Command("type")]
        public async Task Type()
        {
            await Context.Channel.TriggerTypingAsync();
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

        [Command("av")]
        public async Task Avatar(SocketGuildUser user = null)
        {
            if (user == null)
            {
                var builder = new EmbedBuilder()
                    .WithTitle($"Avatar for {(Context.Message.Author as SocketGuildUser).Nickname ?? Context.User.Username}")
                    .WithColor(Color.DarkBlue)
                    .WithImageUrl(Context.Message.Author.GetAvatarUrl(ImageFormat.Auto, 1024) ?? Context.Message.Author.GetDefaultAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithFooter(Context.Guild.Name.ToString());
                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
                return;
            }
            else
            {

                var builder = new EmbedBuilder()
                        .WithTitle($"Avatar for {user.Nickname ?? user.Username}")
                        .WithColor(Color.DarkBlue)
                        .WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 1024) ?? user.GetDefaultAvatarUrl())
                        .WithCurrentTimestamp()
                        .WithFooter(Context.Guild.Name.ToString());
                var embed = builder.Build();
                await ReplyAsync(null, false, embed);
            }
        }

        [Command("meme")]
        [Alias("reddit")]
        public async Task Meme(string subreddit = null)
        {
            await Context.Channel.TriggerTypingAsync();

            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("This subreddit doesn't exist!");
                return;
            }
            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(Color.DarkBlue)
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"🗨️ {post["num_comments"]} ⬆️ {post["ups"]}")
                .WithCurrentTimestamp();
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
    }
}
