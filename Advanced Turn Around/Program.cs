#region

using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Advanced_Turn_Around
{
    internal class Program
    {
        private static readonly List<ChampionInfo> ExistingChampions = new List<ChampionInfo>();

        public static Menu Config;

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
            foreach (ChampionInfo Champ in ExistingChampions)
            {
                Config.SubMenu("CAS").AddSubMenu(new Menu(Champ.CharName + "'s Spells to Avoid", Champ.CharName));
                Config.SubMenu("CAS")
                    .SubMenu(Champ.CharName)
                    .AddItem(new MenuItem(Champ.Key, Champ.SpellName).SetValue(true));
            }

            Config.AddToMainMenu();

            Game.PrintChat(
                "<font color=\"#00BFFF\">Advanced Turn Around# -</font> <font color=\"#FFFFFF\">Loaded</font>");

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Config.Item("Enabled").GetValue<bool>() ||
                (ObjectManager.Player.ChampionName == "Teemo" && !ObjectManager.Player.IsTargetable))
                return;

            if (unit != null && unit.Team != ObjectManager.Player.Team)
            {
                foreach (ChampionInfo Champ in ExistingChampions)
                {
                    if (Config.SubMenu(Champ.CharName).Item(Champ.Key).GetValue<bool>())
                    {
                        if (args.SData.Name.Contains(Champ.Key) &&
                            (ObjectManager.Player.Distance(unit) <= Champ.Range || args.Target == ObjectManager.Player))
                            Packet.C2S.Move.Encoded(
                                new Packet.C2S.Move.Struct(
                                    ObjectManager.Player.Position.X +
                                    ((unit.Position.X - ObjectManager.Player.Position.X)*(Champ.Variable)/
                                     ObjectManager.Player.Distance(unit)),
                                    ObjectManager.Player.Position.Y +
                                    ((unit.Position.Y - ObjectManager.Player.Position.Y)*(Champ.Variable)/
                                     ObjectManager.Player.Distance(unit)))).Send();
                    }
                }
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