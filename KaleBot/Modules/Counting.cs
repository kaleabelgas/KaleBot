using Discord.Commands;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.IO;
using Newtonsoft.Json;

namespace KaleBot.Modules
{
    public class Counting : ModuleBase<SocketCommandContext>
    {
        [RequireOwner]
        [Command("setupcount")]
        public async Task SetupCount()
        {
            if (!Context.Channel.Name.Equals("counting"))
            {
                await ReplyAsync("You need to setup a channel called counting to continue!");
                return;
            }
            GamesData.CountingContext = Context;
            GamesData.CountingContext.Client.MessageReceived += CountManager;
            GamesData.CountingChannel = Context.Channel;
            await ReplyAsync($"Setup Successful! Next number: {GamesData.LastNumber + 1}");
        }

        public async Task CountManager(SocketMessage msg)
        {
            if (msg.Channel != GamesData.CountingChannel) return;
            if (msg.Content.StartsWith('?')) return;
            if (int.TryParse(msg.Content, out int num))
            {
                if(num != GamesData.LastNumber + 1)
                {
                    var emoji = new Emoji("❌");
                    await msg.AddReactionAsync(emoji);
                    GamesData.LastNumber = 0;
                    await ReplyAsync($"Aww shucks, {msg.Author.Mention} broke the chain! Next number: {GamesData.LastNumber + 1}");
                    File.WriteAllText(@"counting.json", JsonConvert.SerializeObject(GamesData.LastNumber));

                }
                else
                {
                    var emoji = new Emoji("✅");
                    await msg.AddReactionAsync(emoji);
                    GamesData.LastNumber++;

                    File.WriteAllText(@"counting.json", JsonConvert.SerializeObject(GamesData.LastNumber));
                }
            }
        }
    }
}
