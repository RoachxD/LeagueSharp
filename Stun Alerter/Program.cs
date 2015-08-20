using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

namespace Stun_Alerter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoadThread = new Thread(Game_OnGameLoad);
                onGameLoadThread.Start();
            };
        }

        private static void Game_OnGameLoad()
        {
            Stunner.InitializeSpells();

            Variable.Config = new Menu("Roach's Stun Alerter#", "RSA", true);

            Variable.Config.AddSubMenu(new Menu("Settings", "Settings"));
            Variable.Config.SubMenu("Settings")
                .AddItem(new MenuItem("Range", "Range: ").SetValue(new Slider(1000, 500, 5000)));
            Variable.Config.SubMenu("Settings")
                .AddItem(
                    new MenuItem("Mode", "Mode: ").SetValue(new StringList(new[] {"In Range", "All over the map"}, 1)));
            Variable.Config.SubMenu("Settings")
                .AddItem(new MenuItem("Pings", "Ping when all Stuns are Unavailable").SetValue(true));
            Variable.Config.AddSubMenu(new Menu("Spells", "Spells"));
            HeroManager.Enemies.ForEach(
                hero =>
                {
                    if (!Variable.StunSpells.ContainsKey(hero.ChampionName))
                    {
                        return;
                    }

                    Variable.Config.SubMenu("Spells").AddSubMenu(new Menu(hero.ChampionName, hero.ChampionName));
                    foreach (var spell in Variable.StunSpells[hero.ChampionName])
                    {
                        Variable.Config.SubMenu("Spells")
                            .SubMenu(hero.ChampionName)
                            .AddItem(
                                new MenuItem(hero.ChampionName + spell.ToString(), hero.ChampionName + spell.ToString())
                                    .SetValue(true));
                    }
                });

            Variable.Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Variable.Config.SubMenu("Drawings").AddSubMenu(new Menu("Panel Position", "Panel"));
            Variable.Config.SubMenu("Drawings")
                .SubMenu("Panel")
                .AddItem(new MenuItem("PosX", "Pos X: ").SetValue(new Slider(20, Drawing.Width, 0)));
            Variable.Config.SubMenu("Drawings")
                .SubMenu("Panel")
                .AddItem(new MenuItem("PosY", "Pos Y: ").SetValue(new Slider(20, Drawing.Height, 0)));
            Variable.Config.SubMenu("Drawings").AddSubMenu(new Menu("Colors", "Colors"));
            Variable.Config.SubMenu("Drawings")
                .SubMenu("Colors")
                .AddItem(new MenuItem("Title", "Title").SetValue(Color.FromArgb(255, 152, 39, 210)));
            Variable.Config.SubMenu("Drawings")
                .SubMenu("Colors")
                .AddItem(new MenuItem("Champions", "Champions").SetValue(Color.FromArgb(255, 255, 255, 255)));
            Variable.Config.SubMenu("Drawings")
                .SubMenu("Colors")
                .AddItem(new MenuItem("Spells", "Spells").SetValue(Color.FromArgb(255, 255, 255, 255)));
            Variable.Config.SubMenu("Drawings")
                .SubMenu("Colors")
                .AddItem(new MenuItem("StatusA", "Status: Available").SetValue(Color.FromArgb(255, 0, 128, 0)));
            Variable.Config.SubMenu("Drawings")
                .SubMenu("Colors")
                .AddItem(new MenuItem("StatusU", "Status: Unavailable").SetValue(Color.FromArgb(255, 128, 128, 128)));
            Variable.Config.SubMenu("Drawings")
                .SubMenu("Colors")
                .AddItem(new MenuItem("Range", "Range Circle").SetValue(Color.FromArgb(255, 255, 255, 255)));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("DisableAll", "Disable All").SetValue(false));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("Title", "Draw Title").SetValue(true));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("Spells", "Draw Spells").SetValue(true));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("Status", "Draw Spells' Status").SetValue(true));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("Range", "Draw Range").SetValue(true));

            Variable.Config.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("<font color=\"#9827D2\">Stun Alerter# -</font> <font color=\"#FFFFFF\">Loaded</font>");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var pings = Variable.Config.SubMenu("Settings").Item("Pings").GetValue<bool>();
            if (!pings)
            {
                return;
            }

            if (
                HeroManager.Enemies.All(
                    hero =>
                        Variable.StunSpells.ContainsKey(hero.ChampionName) &&
                        Variable.StunSpells[hero.ChampionName].All(
                            spell =>
                                (!hero.CanStun(spell) &&
                                 Variable.Config.SubMenu("Spells")
                                     .SubMenu(hero.ChampionName)
                                     .Item(hero.ChampionName + spell.ToString())
                                     .GetValue<bool>()) ||
                                !Variable.Config.SubMenu("Spells")
                                    .SubMenu(hero.ChampionName)
                                    .Item(hero.ChampionName + spell.ToString())
                                    .GetValue<bool>())))
            {
                Internal.Ping(Variable.Player.Position.To2D());
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var disableAll = Variable.Config.SubMenu("Drawings").Item("DisableAll").GetValue<bool>();
            if (disableAll)
            {
                return;
            }

            var title = Variable.Config.SubMenu("Drawings").Item("Title").GetValue<bool>();
            var posX = Variable.Config.SubMenu("Drawings").SubMenu("Panel").Item("PosX").GetValue<Slider>().Value;
            var posY = Variable.Config.SubMenu("Drawings").SubMenu("Panel").Item("PosY").GetValue<Slider>().Value;
            if (title)
            {
                var titleColor = Variable.Config.SubMenu("Drawings").SubMenu("Colors").Item("Title").GetValue<Color>();
                Variable.FontTitle.DrawText(null, "List of Champions: ", posX, posY,
                    Internal.SharpDXConverter(titleColor));
            }

            var extraPosY = 12;
            var range = Variable.Config.SubMenu("Settings").Item("Range").GetValue<Slider>().Value;
            var mode = Variable.Config.SubMenu("Settings").Item("Mode").GetValue<StringList>().SelectedIndex;
            HeroManager.Enemies.ForEach(
                hero =>
                {
                    if (!Variable.StunSpells.ContainsKey(hero.ChampionName))
                    {
                        return;
                    }

                    if ((mode == 0 && hero.Distance(Variable.Player) > range) || hero.IsDead)
                    {
                        return;
                    }

                    var championColor =
                        Variable.Config.SubMenu("Drawings").SubMenu("Colors").Item("Champions").GetValue<Color>();
                    Variable.FontChampion.DrawText(null, hero.ChampionName, posX + 15, posY + extraPosY,
                        Internal.SharpDXConverter(championColor));
                    var drawSpells = Variable.Config.SubMenu("Drawings").Item("Spells").GetValue<bool>();
                    if (drawSpells)
                    {
                        foreach (
                            var spell in
                                Variable.StunSpells[hero.ChampionName].Where(
                                    spell =>
                                        Variable.Config.SubMenu("Spells")
                                            .SubMenu(hero.ChampionName)
                                            .Item(hero.ChampionName + spell.ToString())
                                            .GetValue<bool>()))
                        {
                            extraPosY += 11;

                            var statusAColor =
                                Variable.Config.SubMenu("Drawings").SubMenu("Colors").Item("StatusA").GetValue<Color>();
                            var statusUColor =
                                Variable.Config.SubMenu("Drawings").SubMenu("Colors").Item("StatusU").GetValue<Color>();
                            var spellColor =
                                Variable.Config.SubMenu("Drawings").SubMenu("Colors").Item("Spells").GetValue<Color>();
                            Variable.FontSpell.DrawText(null, hero.ChampionName + spell.ToString(), posX + 25,
                                posY + extraPosY, Internal.SharpDXConverter(spellColor));

                            var drawStatus = Variable.Config.SubMenu("Drawings").Item("Status").GetValue<bool>();
                            if (drawStatus)
                            {
                                using (var graphics = Graphics.FromImage(new Bitmap(1, 1)))
                                {
                                    var size = graphics.MeasureString(hero.ChampionName + spell.ToString(),
                                        new Font("Tahoma", 12, FontStyle.Regular, GraphicsUnit.Pixel));
                                    Variable.FontSpell.DrawText(null,
                                        (hero.CanStun(spell) ? ": Available" : ": Unavailable"),
                                        (int) (posX + size.Width + 15), posY + extraPosY,
                                        hero.CanStun(spell)
                                            ? Internal.SharpDXConverter(statusAColor)
                                            : Internal.SharpDXConverter(statusUColor));
                                }
                            }
                        }
                    }

                    extraPosY += 12;
                });

            var rangeDraw = Variable.Config.SubMenu("Drawings").Item("Range").GetValue<bool>();
            if (rangeDraw && mode == 1)
            {
                var rangeColor = Variable.Config.SubMenu("Drawings").SubMenu("Colors").Item("Range").GetValue<Color>();
                Utility.DrawCircle(Variable.Player.Position, range, rangeColor, 1, 23, true);
            }
        }
    }
}