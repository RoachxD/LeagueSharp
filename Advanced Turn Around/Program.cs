#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace Advanced_Turn_Around
{
    internal class Program
    {
        private static readonly List<ChampionInfo> ExistingChampions = new List<ChampionInfo>();
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            Game.OnGameStart += Game_OnGameStart;
        }

        private static void Game_OnGameStart(EventArgs args)
        {
            AddChampions();

            Config = new Menu("Advanced Turn Around", "ATA", true);

            Config.AddItem(new MenuItem("Enable", "Enable the Script").SetValue(true));

            Config.AddSubMenu(new Menu("Champions and Spells", "CAS"));
            foreach (var champ in ExistingChampions)
            {
                Config.SubMenu("CAS").AddSubMenu(new Menu(champ.CharName + "'s Spells to Avoid", champ.CharName));
                Config.SubMenu("CAS")
                    .SubMenu(champ.CharName)
                    .AddItem(new MenuItem(champ.Key, champ.SpellName).SetValue(true));
            }

            Config.AddToMainMenu();

            Game.PrintChat(
                "<font color=\"#00BFFF\">Advanced Turn Around# -</font> <font color=\"#FFFFFF\">Loaded</font>");

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Config.Item("Enabled").GetValue<bool>() ||
                (Player.ChampionName == "Teemo" && !Player.IsTargetable))
            {
                return;
            }

            if (unit == null || unit.Team == Player.Team)
            {
                return;
            }

            foreach (
                var vector in
                    ExistingChampions.Where(champ => Config.SubMenu(champ.CharName).Item(champ.Key).GetValue<bool>())
                        .Where(
                            champ =>
                                args.SData.Name.Contains(champ.Key) &&
                                (Player.Distance(unit) <= champ.Range || args.Target == Player))
                        .Select(
                            champ =>
                                new Vector3(
                                    Player.Position.X +
                                    ((unit.Position.X - Player.Position.X)*(champ.Variable)/Player.Distance(unit)),
                                    Player.Position.Y +
                                    ((unit.Position.Y - Player.Position.Y)*(champ.Variable)/Player.Distance(unit)), 0)))
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, vector);
            }
        }

        private static void AddChampions()
        {
            ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "Cassiopeia",
                    Key = "CassiopeiaPetrifyingGaze",
                    Range = 750,
                    SpellName = "Petrifying Gaze (R)",
                    Variable = -100
                });

            ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "Shaco",
                    Key = "TwoShivPoison",
                    Range = 625,
                    SpellName = "Two-Shiv Poison (E)",
                    Variable = 100
                });

            ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "Tryndamere",
                    Key = "MockingShout",
                    Range = 850,
                    SpellName = "Mocking Shout (W)",
                    Variable = 100
                });
        }

        internal class ChampionInfo
        {
            public string CharName { get; set; }
            public string Key { get; set; }
            public float Range { get; set; }
            public string SpellName { get; set; }
            public int Variable { get; set; }
        }
    }
}