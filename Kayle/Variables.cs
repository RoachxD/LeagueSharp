#region

using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Kayle
{
    internal class Variable
    {
        public const string ChampionName = "Kayle";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        public static Items.Item Dfg;
        public static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static Menu Config;

        public static bool RighteousFuryActive
        {
            get { return ObjectManager.Player.AttackRange > 125f; }
        }
    }
}