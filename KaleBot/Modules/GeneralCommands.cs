using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using KaleBot.Services;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace KaleBot.Modules
{
    public class GeneralCommands : ModuleBase<SocketCommandContext>
    {
        [Command("cc")]
        public async Task CustomCommand([Remainder]string command = null)
        {
            if (string.IsNullOrEmpty(command))
            {
                await ReplyAsync("Command provided is empty.");
                return;
            }
            if(!CommandHandler.UserCommands.ContainsKey(command))
            {
                await ReplyAsync("Custom command does not exist!");
                return;
            }
            var content = CommandHandler.UserCommands[command];
            await ReplyAsync(CommandHandler.UserCommands[command]);
        }


        [Command("addcc")]
        public async Task AddCommand(string command, [Remainder]string reply = null)
        {
            if(reply == null)
            {
                await ReplyAsync("Command cannot be empty.");
                return;
            }
            await ReplyAsync("Command added.");

            CommandHandler.UserCommands.Add(command, reply);

            var path = @"customcommands.json";

            File.WriteAllText(path, JsonConvert.SerializeObject(CommandHandler.UserCommands));
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

        [Command("Harass")]
        public async Task StartHarass(ulong id, string message)
        {
            if (Context.Message.MentionedRoles.Count > 0 || Context.Message.MentionedEveryone)
            {
                await ReplyAsync(Context.User.Mention + " tried to ping roles smh");
                return;
            }
            var isEmote = Emote.TryParse(message, out _);
            if (!isEmote)
            {
                await ReplyAsync("You need to provide an actual emote, dude!");
                return;
            }
            await ReplyAsync("Your wish is my command.");
            var path = @"harasslist.json";


            if (CommandHandler.HarassList.ContainsKey(id))
            {
                CommandHandler.HarassList[id].HarassEmote = message;
                CommandHandler.HarassList[id].Harasser = Context.Message.Author.Id;

                File.WriteAllText(path, JsonConvert.SerializeObject(CommandHandler.HarassList));
                return;
            }

            var valuePair = new ValuePair
            {
                HarassEmote = message,
                Harasser = Context.Message.Author.Id      
            };


            CommandHandler.HarassList.Add(id, valuePair);

            File.WriteAllText(path, JsonConvert.SerializeObject(CommandHandler.HarassList));
        }

        [Command("Harassers")]
        public async Task Harassers()
        {
            var builder = new EmbedBuilder()
                .WithTitle("Current Harass List")
                .WithCurrentTimestamp();
            foreach(var value in CommandHandler.HarassList.Keys)
            {
                var harasser = Context.Client.GetUser(CommandHandler.HarassList[value].Harasser);
                var harassed = Context.Client.GetUser(value);
                var emote = CommandHandler.HarassList[value].HarassEmote;

                builder.AddField(harassed.Username, $"Harasser: {harasser.Mention}\n" +
                    $"Emote: {Emote.Parse(emote)}");
            }
            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

        [Command("love")]
        public async Task EndHarass(ulong id)
        {
            var removed = CommandHandler.HarassList.Remove(id);
            var path = @"harasslist.json";
            if (!removed)
            {
                await ReplyAsync("User not in harass list!");
                return;
            }
            await ReplyAsync("Loved!");
            File.WriteAllText(path, JsonConvert.SerializeObject(CommandHandler.HarassList));
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
            searchRequest.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.None;
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
            searchRequest.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.None;
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

            await ReplyAsync($"https://www.youtube.com/watch?v={searchResponse.Items[0].Id.VideoId}");
        }


        [Command("snipe")]
        public async Task Snipe()
        {
            if (CommandHandler.LastMessage == null)
            {
                await ReplyAsync("There's nothing to snipe!");
                return;
            }

            var builder = new EmbedBuilder()
                .WithTitle($"Deleted message by {CommandHandler.LastMessage.Author.Username}")
                .WithDescription($"{CommandHandler.LastMessage.Content}")
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp()
                .WithFooter(Context.Guild.Name.ToString());

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
        }

        [Command("editsnipe")]
        public async Task EditSnipe()
        {
            if (CommandHandler.LastMessageEdit == null)
            {
                await ReplyAsync("There's nothing to snipe!");
                return;
            }
            var builder = new EmbedBuilder()
                .WithTitle($"Edited message by {CommandHandler.LastMessageEdit.Author.Username}")
                .WithDescription($"{CommandHandler.LastMessageEdit.Content}")
                .WithColor(Color.DarkBlue)
                .WithCurrentTimestamp()
                .WithFooter(Context.Guild.Name.ToString());

            var embed = builder.Build();

            await ReplyAsync(null, false, embed);
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
       

        [Command("type")]
        public async Task Type()
        {
            await Context.Channel.TriggerTypingAsync();
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
