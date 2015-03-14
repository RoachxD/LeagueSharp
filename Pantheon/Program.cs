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
                .AddItem(new MenuItem("Combo_Key", "Full Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Variable.Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("Combo_Mode", "Combo Mode", true).SetValue(
                        new StringList(new[]
                        {"Normal (Q-W-E with No Restrictions)", "Ganking (W-E-Q - Will not E until target immovable)"})));
            Variable.Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("Combo_Switch", "Switch mode Key", true).SetValue(new KeyBind("T".ToCharArray()[0],
                        KeyBindType.Press)));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("Combo_Items", "Use Items with Burst").SetValue(true));
            if (Variable.SmiteSlot != SpellSlot.Unknown)
            {
                Variable.Config.SubMenu("Combo")
                    .AddItem(new MenuItem("Auto_Smite", "Use Smite on Target if QWE Available").SetValue(true));
            }

            if (Variable.IgniteSlot != SpellSlot.Unknown)
            {
                Variable.Config.SubMenu("Combo")
                    .AddItem(
                        new MenuItem("Auto_Ignite", "Use Ignite with Burst").SetValue(
                            new StringList(new[] {"Burst", "KS"})));
            }

            Variable.Config.AddSubMenu(new Menu("Harass Settings", "Harass"));
            Variable.Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("Harass_Key", "Harass Key").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Press)));
            Variable.Config.SubMenu("Harass")
                .AddItem(new MenuItem("Harass_Mode", "Harass Mode: ").SetValue(new StringList(new[] {"Q", "W+E"})));
            Variable.Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("Auto_Q", "Auto-Q when Target in Range").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Variable.Config.SubMenu("Harass")
                .AddItem(new MenuItem("Auto_Q_Turret", "Don't Auto-Q if in enemy Turret Range").SetValue(true));
            Variable.Config.SubMenu("Harass")
                .AddItem(new MenuItem("Harass_Mana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            Variable.Config.AddSubMenu(new Menu("Farming Settings", "Farm"));
            Variable.Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("Farm_Key", "Farming Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("Farm_Q", "Farm with Spear Shot (Q)").SetValue(true));
            Variable.Config.SubMenu("Farm")
                .AddItem(new MenuItem("Farm_W", "Farm with Aegis of Zeonia (W)").SetValue(true));
            Variable.Config.SubMenu("Farm")
                .AddItem(new MenuItem("Farm_Mana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            Variable.Config.AddSubMenu(new Menu("Jungle Clear Settings", "Jungle"));
            Variable.Config.SubMenu("Jungle")
                .AddItem(
                    new MenuItem("Jungle_Key", "Jungle Clear Key").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));
            Variable.Config.SubMenu("Jungle")
                .AddItem(new MenuItem("Jungle_Q", "Farm with Spear Shot (Q)").SetValue(true));
            Variable.Config.SubMenu("Jungle")
                .AddItem(new MenuItem("Jungle_W", "Farm with Aegis of Zeonia (W)").SetValue(true));
            Variable.Config.SubMenu("Jungle")
                .AddItem(new MenuItem("Jungle_E", "Farm with Heartseeker Strike (E)").SetValue(true));

            Variable.Config.AddSubMenu(new Menu("Draw Settings", "Drawing"));
            Variable.Config.SubMenu("Drawing")
                .AddItem(new MenuItem("No_Drawings", "Disable All Range Draws").SetValue(false));
            Variable.Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("Target", "Draw Circle on Target").SetValue(new Circle(true,
                        Color.FromArgb(255, 255, 0, 0))));
            Variable.Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("Draw_Q", "Draw Spear Shot (Q) Range").SetValue(new Circle(true,
                        Color.FromArgb(255, 178, 0, 0))));
            Variable.Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("Draw_W", "Draw Aegis of Zeonia (W) Range").SetValue(new Circle(false,
                        Color.FromArgb(255, 32, 178, 170))));
            Variable.Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("Draw_E", "Draw Heartseeker Strike (E) Range").SetValue(new Circle(true,
                        Color.FromArgb(255, 128, 0, 128))));
            Variable.Config.SubMenu("Drawing")
                .AddItem(new MenuItem("Current_Combo_Mode", "Draw Current Combo Mode").SetValue(true));

            Variable.Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            Variable.Config.SubMenu("Misc")
                .AddItem(new MenuItem("Stop_Channel", "Interrupt Channeling Spells").SetValue(true));
            Variable.Config.SubMenu("Misc")
                .AddItem(new MenuItem("Use_Packets", "Use Packets to Cast Spells").SetValue(false));

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
            var comboKey = Variable.Config.Item("Combo_Key").GetValue<KeyBind>().Active;
            var harassKey = Variable.Config.Item("Harass_Key").GetValue<KeyBind>().Active;
            var farmKey = Variable.Config.Item("Farm_Key").GetValue<KeyBind>().Active;
            var jungleClearKey = Variable.Config.Item("Jungle_Key").GetValue<KeyBind>().Active;

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

                if (!Variable.Config.Item("Auto_Q").GetValue<KeyBind>().Active || target == null)
                {
                    return;
                }

                if (Variable.Config.Item("Auto_Q_Turret").GetValue<bool>()
                    ? !Variable.Player.UnderTurret(true)
                    : Variable.Player.UnderTurret(true) && Variable.Player.Distance(target) <= Variable.Q.Range &&
                      Variable.Q.IsReady())
                {
                    Variable.Q.CastOnUnit(target, Variable.Config.Item("Use_Packets").GetValue<bool>());
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Variable.Config.Item("No_Drawings").GetValue<bool>())
            {
                return;
            }

            foreach (
                var spell in
                    Variable.Spells.Where(spell => Variable.Config.Item("Draw_" + spell.Slot).GetValue<Circle>().Active)
                )
            {
                Utility.DrawCircle(Variable.Player.Position, spell.Range,
                    Variable.Config.Item(spell.Slot + "Draw").GetValue<Circle>().Color);
            }

            var target = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Physical);
            if (Variable.Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Utility.DrawCircle(target.Position, 50, Variable.Config.Item("Target").GetValue<Circle>().Color, 1, 50);
            }

            if (!Variable.Config.Item("Current_Combo_Mode", true).GetValue<bool>())
            {
                return;
            }

            var worldToScreen = Drawing.WorldToScreen(Variable.Player.Position);
            var comboMode = Variable.Config.Item("Combo_Mode", true).GetValue<StringList>().SelectedIndex;
            switch (comboMode)
            {
                case 0:
                    Drawing.DrawText(worldToScreen[0] - 20, worldToScreen[1], Color.White,
                        "Normal (Q-W-E with No Restrictions)");
                    break;
                case 1:
                    Drawing.DrawText(worldToScreen[0] - 20, worldToScreen[1], Color.White,
                        "Ganking (W-E-Q - Will not E until target immovable)");
                    break;
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Variable.Config.Item("Stop_Channel").GetValue<bool>())
            {
                return;
            }

            if (!(Variable.Player.Distance(unit) <= Variable.W.Range) || !Variable.W.IsReady())
            {
                return;
            }

            Variable.W.CastOnUnit(unit, Variable.Config.Item("Use_Packets").GetValue<bool>());
        }
    }
}