using System.Data;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaleBot.Services;
using System;
using System.Linq;

namespace KaleBot.Modules
{
    public class Games : ModuleBase<SocketCommandContext>
    {
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

        

        [Command("Guess")]
        public async Task Guess(int numLength = 1)
        {
            if (GamesData.GuessGameOngoing)
            {
                await ReplyAsync("A game is ongoing!");
                return;
            }

            if (numLength < 1)
            {
                await ReplyAsync("Number of digits cannot be negative or zero.");
                return;
            }
            if (numLength > 10)
            {
                await ReplyAsync($"Dude, too much! Number cannot be more than {int.MaxValue}");
                return;
            }

            GamesData.GuessGameOngoing = true;
            GamesData.GuessContext = Context;
            GamesData.GuessingChannel = GamesData.GuessContext.Channel;

            var random = new Random();

            GamesData.NumberToGuess = random.Next(0, (int)Math.Pow(10, numLength) - 1);

            GamesData.GuessContext.Client.MessageReceived += GuessListener;

            await ReplyAsync($"Guessing game started. Number length: {numLength} digit.");

        }

        [Command("Guess")]
        public async Task Guess(string param)
        {
            if (param.Equals("stop"))
            {
                await ReplyAsync($"Game stopped. Number: {GamesData.NumberToGuess}");
                GamesData.GuessGameOngoing = false;
                GamesData.GuessContext.Client.MessageReceived -= GuessListener;
                return;
            }
        }

        private async Task GuessListener(SocketMessage arg)
        {
            //await Task.Delay(100);
            if (arg.Content.StartsWith("?")) return;
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            if (!GamesData.GuessGameOngoing) return;
            if (arg.Channel != GamesData.GuessingChannel) return;

            if (int.TryParse(arg.Content, out int num))
            {
                if (num == GamesData.NumberToGuess)
                {
                    await ReplyAsync("You got it right! Num: " + GamesData.NumberToGuess);
                    GamesData.GuessContext.Client.MessageReceived -= GuessListener;
                    GamesData.GuessGameOngoing = false;
                }
                else
                {
                    string numberIs = "";

                    if (num > GamesData.NumberToGuess)
                    {
                        numberIs = "lower.";
                    }
                    else
                    {
                        numberIs = "higher.";
                    }

                    await ReplyAsync($"Try again! Number is: {numberIs}");
                }
                return;
            }

            //string exception = "";

            //try
            //{
            //    int numbr = int.Parse(arg.Content);
            //}
            //catch(Exception ex)
            //{
            //    exception = ex.ToString();
            //}

            //exception = exception.Split(new[] { '\r', '\n' }).FirstOrDefault();

            //await ReplyAsync($"That's not a number! Reason: {exception}");
        }

        [Command("lightshot")]
        public async Task LightShot(int screenshots)
        {
            var random = new Random();

            for (int i = 0; i < screenshots; i++)
            {
                string text = "";
                for (int j = 0; j < 6; j++)
                {
                    int index = random.Next(0, GamesData.AlphaNumeric.Length);
                    text += GamesData.AlphaNumeric[index];
                }
                await ReplyAsync($"https://prnt.sc/{text}");
                await Task.Delay(500);
            }
        }
        [Command("ytr")]
        public async Task YoutubeRandom(int amount = 1)
        {
            var random = new Random();

            for (int i = 0; i < amount; i++)
            {
                string text = "";
                for (int j = 0; j < 11; j++)
                {
                    int index = random.Next(0, GamesData.AlphaNumeric.Length);
                    text += GamesData.AlphaNumeric[index];
                }
                await ReplyAsync($"https://www.youtube.com/watch?v={text}");
                await Task.Delay(1500);
            }
        }
    }

}
