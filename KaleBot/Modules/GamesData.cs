using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaleBot.Modules
{
    public class GamesData
    {
        public static int NumberToGuess { get; set; }
        public static bool GuessGameOngoing { get; set; }
        public static ISocketMessageChannel GuessingChannel { get; set; }
        public static SocketCommandContext GuessContext { get; set; }

        public static SocketCommandContext CountingContext { get; set; }
        public static int LastNumber { get; set; } = 0;
        public static ISocketMessageChannel CountingChannel { get; set; }


        public static char[] AlphaNumeric { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    }
}
