using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Direct3D9;

namespace Stun_Alerter
{
    internal class Variable
    {
        public static readonly Font FontSpell = new Font(Drawing.Direct3DDevice,
            new FontDescription
            {
                FaceName = "Tahoma",
                Height = 12,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.ClearTypeNatural
            });

        public static readonly Font FontChampion = new Font(Drawing.Direct3DDevice,
            new FontDescription
            {
                FaceName = "Tahoma",
                Height = 13,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.ClearTypeNatural
            });

        public static readonly Font FontTitle = new Font(Drawing.Direct3DDevice,
            new FontDescription
            {
                FaceName = "Tahoma",
                Height = 13,
                Weight = FontWeight.Bold,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.ClearTypeNatural
            });

        public static Dictionary<string, List<SpellSlot>> StunSpells = new Dictionary<string, List<SpellSlot>>();
        public static Menu Config { get; set; }
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static int LastPing = 0;
    }
}