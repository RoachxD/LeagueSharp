#region

using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Pantheon
{
    internal class Program
    {
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
            if (Variable.Player.BaseSkinName != Variable.CharName)
            {
                return;
            }

            Variable.Q = new Spell(SpellSlot.Q, 600);
            Variable.W = new Spell(SpellSlot.W, 600);
            Variable.E = new Spell(SpellSlot.E, 700);

            Internal.SetIgniteSlot();
            Internal.SetSmiteSlot();

            Variable.Spells.Add(Variable.Q);
            Variable.Spells.Add(Variable.W);
            Variable.Spells.Add(Variable.E);

            Variable.Config = new Menu(Variable.CharName, Variable.CharName, true);

            Variable.Config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            Variable.Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboKey", "Full Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Variable.Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboMode", "Combo Mode").SetValue(
                        new StringList(new[]
                        {"Normal (Q-W-E with No Restrictions)", "Ganking (W-E-Q - Will not E until target immovable)"})));
            Variable.Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboSwitch", "Switch mode Key").SetValue(new KeyBind("T".ToCharArray()[0],
                        KeyBindType.Press)));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("ComboItems", "Use Items with Burst").SetValue(true));
            if (Variable.SmiteSlot != SpellSlot.Unknown)
            {
                Variable.Config.SubMenu("Combo")
                    .AddItem(new MenuItem("AutoSmite", "Use Smite on Target if QWE Available").SetValue(true));
            }

            if (Variable.IgniteSlot != SpellSlot.Unknown)
            {
                Variable.Config.SubMenu("Combo")
                    .AddItem(
                        new MenuItem("AutoIgnite", "Use Ignite with Burst").SetValue(
                            new StringList(new[] {"Burst", "KS"})));
            }

            Variable.Config.AddSubMenu(new Menu("Harass Settings", "Harass"));
            Variable.Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassKey", "Harass Key").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Press)));
            Variable.Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassMode", "Harass Mode: ").SetValue(new StringList(new[] {"Q", "W+E"})));
            Variable.Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("AutoQ", "Auto-Q when Target in Range").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Variable.Config.SubMenu("Harass")
                .AddItem(new MenuItem("AutoQTurret", "Don't Auto-Q if in enemy Turret Range").SetValue(true));
            Variable.Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassMana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            Variable.Config.AddSubMenu(new Menu("Farming Settings", "Farm"));
            Variable.Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FarmKey", "Farming Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("FarmQ", "Farm with Spear Shot (Q)").SetValue(true));
            Variable.Config.SubMenu("Farm")
                .AddItem(new MenuItem("FarmW", "Farm with Aegis of Zeonia (W)").SetValue(true));
            Variable.Config.SubMenu("Farm")
                .AddItem(new MenuItem("FarmMana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            Variable.Config.AddSubMenu(new Menu("Jungle Clear Settings", "Jungle"));
            Variable.Config.SubMenu("Jungle")
                .AddItem(
                    new MenuItem("JungleKey", "Jungle Clear Key").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));
            Variable.Config.SubMenu("Jungle")
                .AddItem(new MenuItem("JungleQ", "Farm with Spear Shot (Q)").SetValue(true));
            Variable.Config.SubMenu("Jungle")
                .AddItem(new MenuItem("JungleW", "Farm with Aegis of Zeonia (W)").SetValue(true));
            Variable.Config.SubMenu("Jungle")
                .AddItem(new MenuItem("JungleE", "Farm with Heartseeker Strike (E)").SetValue(true));

            Variable.Config.AddSubMenu(new Menu("Draw Settings", "Drawing"));
            Variable.Config.SubMenu("Drawing")
                .AddItem(new MenuItem("NoDrawings", "Disable All Range Draws").SetValue(false));
            Variable.Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("Target", "Draw Circle on Target").SetValue(new Circle(true,
                        Color.FromArgb(255, 255, 0, 0))));
            Variable.Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("DrawQ", "Draw Spear Shot (Q) Range").SetValue(new Circle(true,
                        Color.FromArgb(255, 178, 0, 0))));
            Variable.Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("DrawW", "Draw Aegis of Zeonia (W) Range").SetValue(new Circle(false,
                        Color.FromArgb(255, 32, 178, 170))));
            Variable.Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("DrawE", "Draw Heartseeker Strike (E) Range").SetValue(new Circle(true,
                        Color.FromArgb(255, 128, 0, 128))));
            Variable.Config.SubMenu("Drawing")
                .AddItem(new MenuItem("CurrentComboMode", "Draw Current Combo Mode").SetValue(true));

            Variable.Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            Variable.Config.SubMenu("Misc")
                .AddItem(new MenuItem("StopChannel", "Interrupt Channeling Spells").SetValue(true));
            Variable.Config.SubMenu("Misc")
                .AddItem(new MenuItem("UsePackets", "Use Packets to Cast Spells").SetValue(false));

            Variable.Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Variable.Orbwalker = new Orbwalking.Orbwalker(Variable.Config.SubMenu("Orbwalking"));

            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Variable.Config.AddSubMenu(tsMenu);

            Variable.Config.AddToMainMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = Internal.ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter_OnPossibleToInterrupt;

            Game.PrintChat("<font color=\"#00BFFF\">Pantheon# -</font> <font color=\"#FFFFFF\">Loaded</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Variable.Player.IsDead)
            {
                return;
            }

            Internal.ComboModeSwitch();

            var target = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Physical);
            var comboKey = Variable.Config.SubMenu("Combo").Item("ComboKey").GetValue<KeyBind>().Active;
            var harassKey = Variable.Config.SubMenu("Harass").Item("HarassKey").GetValue<KeyBind>().Active;
            var farmKey = Variable.Config.SubMenu("Farm").Item("FarmKey").GetValue<KeyBind>().Active;
            var jungleClearKey = Variable.Config.SubMenu("Jungle").Item("JungleKey").GetValue<KeyBind>().Active;

            Variable.Orbwalker.SetAttack(!Internal.UsingEorR());
            Variable.Orbwalker.SetMovement(!Internal.UsingEorR());

            if (comboKey && target != null)
            {
                Internal.Combo(target);
            }
            else
            {
                if (harassKey && target != null)
                {
                    Internal.Harass(target);
                }

                if (farmKey)
                {
                    Internal.Farm();
                }

                if (jungleClearKey)
                {
                    Internal.JungleClear();
                }

                if (!Variable.Config.SubMenu("Harass").Item("AutoQ").GetValue<KeyBind>().Active || target == null)
                {
                    return;
                }

                if (Variable.Config.SubMenu("Harass").Item("AutoQTurret").GetValue<bool>()
                    ? !Variable.Player.UnderTurret(true)
                    : Variable.Player.UnderTurret(true) && Variable.Player.Distance(target) <= Variable.Q.Range &&
                      Variable.Q.IsReady())
                {
                    Variable.Q.CastOnUnit(target, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Variable.Config.SubMenu("Drawing").Item("NoDrawings").GetValue<bool>())
            {
                return;
            }

            foreach (
                var spell in
                    Variable.Spells.Where(spell => Variable.Config.SubMenu("Drawing").Item("Draw" + spell.Slot).GetValue<Circle>().Active)
                )
            {
                Utility.DrawCircle(Variable.Player.Position, spell.Range,
                    Variable.Config.SubMenu("Drawing").Item("Draw" + spell.Slot).GetValue<Circle>().Color);
            }

            var target = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Physical);
            if (Variable.Config.SubMenu("Drawing").Item("Target").GetValue<Circle>().Active && target != null)
            {
                Utility.DrawCircle(target.Position, 50, Variable.Config.Item("Target").GetValue<Circle>().Color, 1, 50);
            }

            if (!Variable.Config.SubMenu("Drawing").Item("CurrentComboMode").GetValue<bool>())
            {
                return;
            }

            var worldToScreen = Drawing.WorldToScreen(Variable.Player.Position);
            var comboMode = Variable.Config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
            switch (comboMode)
            {
                case 0:
                    Drawing.DrawText(worldToScreen[0] - 130, worldToScreen[1], Color.White,
                        "Normal (Q-W-E with No Restrictions)");
                    break;
                case 1:
                    Drawing.DrawText(worldToScreen[0] - 175, worldToScreen[1], Color.White,
                        "Ganking (W-E-Q - Will not E until target immovable)");
                    break;
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Variable.Config.SubMenu("Misc").Item("StopChannel").GetValue<bool>())
            {
                return;
            }

            if (!(Variable.Player.Distance(unit) <= Variable.W.Range) || !Variable.W.IsReady())
            {
                return;
            }

            Variable.W.CastOnUnit(unit, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
        }
    }
}