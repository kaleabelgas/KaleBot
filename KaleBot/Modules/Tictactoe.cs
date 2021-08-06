using Discord.Commands;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaleBot.Modules
{
    public class Tictactoe : ModuleBase<SocketCommandContext>
    {
        private SocketGuildUser _player1;
        private SocketGuildUser _player2;
        private bool _player1Turn;
        private int[,] _board;

        public Tictactoe(SocketGuildUser player1, SocketGuildUser player2)
        {
            _player1 = player1;
            _player2 = player2;
        }

        public async Task ReveiveInput(SocketMessage args)
        {
            if (args.Author != _player1 || args.Author != _player2) return;

            char[] possiblex = { 'a', 'b', 'c' };

            var xpos = args.Content.Substring(0, 1);
            var ypos = args.Content.Substring(1, 1);

            if (!possiblex.Any(x => xpos.Contains(x))) return;
            var parsed = int.TryParse(ypos as string, out int yposOut);
            if (parsed == false) return;

            if (yposOut > 3) return;


            if (_player1Turn && args.Author.Equals(_player1))
            {

            }
            else
            {

            }

            await ReplyAsync(xpos + yposOut.ToString());

            _player1Turn = !_player1Turn;
        }

    }
}
