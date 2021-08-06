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
    }

}
