#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Item_Swapper
{
    internal class Program
    {
        private static byte _firstKey = 0x60;
        private static readonly byte[] Keys = {0x64, 0x65, 0x66, 0x61, 0x62, 0x63};

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("<font color=\"#00BFFF\">Item Swapper# -</font> <font color=\"#FFFFFF\">Loaded</font>");

            Game.OnWndProc += Game_OnWndProc;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
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

            var key = (byte) args.WParam;
            if (_firstKey == 0x60)
            {
                _firstKey = key;
            }

            if (_firstKey == key)
            {
                return;
            }

            Packet.C2S.SwapItem.Encoded(new Packet.C2S.SwapItem.Struct((byte) Array.IndexOf(Keys, _firstKey),
                (byte) Array.IndexOf(Keys, key), ObjectManager.Player.NetworkId)).Send();
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.S2C.SwapItemAns.Header)
            {
                _firstKey = 0x60;
            }
        }
    }
}