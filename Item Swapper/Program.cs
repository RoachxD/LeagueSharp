#region

using System;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Item_Swapper
{
    internal class Program
    {
        private static int _firstKey = 0x60;
        private static readonly int[] Keys = {0x64, 0x65, 0x66, 0x61, 0x62, 0x63};

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoad = new Thread(Game_OnGameLoad);
                onGameLoad.Start();
            };
        }

        private static void Game_OnGameLoad()
        {
            Game.PrintChat("<font color=\"#00BFFF\">Item Swapper# -</font> <font color=\"#FFFFFF\">Loaded</font>");

            Game.OnWndProc += Game_OnWndProc;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x0100 && args.WParam == 0x60)
            {
                _firstKey = 0x60;
            }

            if (args.Msg != 0x0100 || !Keys.ToList().Contains((byte) args.WParam))
            {
                return;
            }

            var key = (int) args.WParam;
            if (_firstKey == 0x60)
            {
                _firstKey = key;
            }

            if (_firstKey == key)
            {
                return;
            }


            ObjectManager.Player.SwapItem(Array.IndexOf(Keys, _firstKey), Array.IndexOf(Keys, key));
            _firstKey = 0x60;
        }
    }
}