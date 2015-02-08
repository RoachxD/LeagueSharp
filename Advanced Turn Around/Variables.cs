#region

using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Advanced_Turn_Around
{
    internal class Variable
    {
        public enum MovementDirection
        {
            Forward = 1,
            Backward = 2
        }

        public static readonly List<Internal.ChampionInfo> ExistingChampions = new List<Internal.ChampionInfo>();
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;
    }
}