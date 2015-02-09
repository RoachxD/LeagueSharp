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

            Variable.Config.AddSubMenu(new Menu("Combo Settings", "combo"));
            Variable.Config.SubMenu("combo")
                .AddItem(new MenuItem("comboKey", "Full Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Variable.Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items with Burst").SetValue(true));
            if (Variable.SmiteSlot != SpellSlot.Unknown)
            {
                Variable.Config.SubMenu("combo")
                    .AddItem(new MenuItem("autoSmite", "Use Smite on Target if QWE Available").SetValue(true));
            }

            if (Variable.IgniteSlot != SpellSlot.Unknown)
            {
                Variable.Config.SubMenu("combo")
                    .AddItem(new MenuItem("autoIgnite", "Use Ignite with Burst").SetValue(true));
            }

            Variable.Config.AddSubMenu(new Menu("Harass Settings", "harass"));
            Variable.Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("harassKey", "Harass Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Variable.Config.SubMenu("harass")
                .AddItem(new MenuItem("hMode", "Harass Mode: ").SetValue(new StringList(new[] {"Q", "W+E"})));
            Variable.Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("autoQ", "Auto-Q when Target in Range").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Variable.Config.SubMenu("harass")
                .AddItem(new MenuItem("aQT", "Don't Auto-Q if in enemy Turret Range").SetValue(true));
            Variable.Config.SubMenu("harass")
                .AddItem(new MenuItem("harassMana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            Variable.Config.AddSubMenu(new Menu("Farming Settings", "farm"));
            Variable.Config.SubMenu("farm")
                .AddItem(
                    new MenuItem("farmKey", "Farming Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Variable.Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Farm with Spear Shot (Q)").SetValue(true));
            Variable.Config.SubMenu("farm")
                .AddItem(new MenuItem("wFarm", "Farm with Aegis of Zeonia (W)").SetValue(true));
            Variable.Config.SubMenu("farm")
                .AddItem(new MenuItem("farmMana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            Variable.Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle"));
            Variable.Config.SubMenu("jungle")
                .AddItem(
                    new MenuItem("jungleKey", "Jungle Clear Key").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));
            Variable.Config.SubMenu("jungle")
                .AddItem(new MenuItem("qJungle", "Farm with Spear Shot (Q)").SetValue(true));
            Variable.Config.SubMenu("jungle")
                .AddItem(new MenuItem("wJungle", "Farm with Aegis of Zeonia (W)").SetValue(true));
            Variable.Config.SubMenu("jungle")
                .AddItem(new MenuItem("eJungle", "Farm with Heartseeker Strike (E)").SetValue(true));

            Variable.Config.AddSubMenu(new Menu("Draw Settings", "drawing"));
            Variable.Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable All Range Draws").SetValue(false));
            Variable.Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("Target", "Draw Circle on Target").SetValue(new Circle(true,
                        Color.FromArgb(255, 255, 0, 0))));
            Variable.Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("QDraw", "Draw Spear Shot (Q) Range").SetValue(new Circle(true,
                        Color.FromArgb(255, 178, 0, 0))));
            Variable.Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("WDraw", "Draw Aegis of Zeonia (W) Range").SetValue(new Circle(false,
                        Color.FromArgb(255, 32, 178, 170))));
            Variable.Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("EDraw", "Draw Heartseeker Strike (E) Range").SetValue(new Circle(true,
                        Color.FromArgb(255, 128, 0, 128))));

            Variable.Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Variable.Config.SubMenu("misc")
                .AddItem(new MenuItem("stopChannel", "Interrupt Channeling Spells").SetValue(true));
            Variable.Config.SubMenu("misc")
                .AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false));

            Variable.Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Variable.Orbwalker = new Orbwalking.Orbwalker(Variable.Config.SubMenu("Orbwalking"));

            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Variable.Config.AddSubMenu(tsMenu);

            Variable.Config.AddToMainMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = Internal.ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += Game_OnProcessSpell;
            GameObject.OnDelete += Game_OnObjectDelete;

            Game.PrintChat("<font color=\"#00BFFF\">Pantheon# -</font> <font color=\"#FFFFFF\">Loaded</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Physical);

            var comboKey = Variable.Config.Item("comboKey").GetValue<KeyBind>().Active;
            var harassKey = Variable.Config.Item("harassKey").GetValue<KeyBind>().Active;
            var farmKey = Variable.Config.Item("farmKey").GetValue<KeyBind>().Active;
            var jungleClearKey = Variable.Config.Item("jungleKey").GetValue<KeyBind>().Active;

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

                if (!Variable.Config.Item("autoQ").GetValue<KeyBind>().Active || target == null)
                {
                    return;
                }

                if (Variable.Config.Item("aQT").GetValue<bool>()
                    ? !Variable.Player.UnderTurret(true)
                    : Variable.Player.UnderTurret(true) && Variable.Player.Distance(target) <= Variable.Q.Range &&
                      Variable.Q.IsReady())
                {
                    Variable.Q.CastOnUnit(target, Variable.Config.Item("usePackets").GetValue<bool>());
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Variable.Config.Item("mDraw").GetValue<bool>())
            {
                return;
            }

            foreach (
                var spell in
                    Variable.Spells.Where(spell => Variable.Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active))
            {
                Utility.DrawCircle(Variable.Player.Position, spell.Range,
                    Variable.Config.Item(spell.Slot + "Draw").GetValue<Circle>().Color);
            }

            var target = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Physical);
            if (Variable.Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Utility.DrawCircle(target.Position, 50, Variable.Config.Item("Target").GetValue<Circle>().Color, 1, 50);
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Variable.Config.Item("stopChannel").GetValue<bool>())
            {
                return;
            }

            if (!(Variable.Player.Distance(unit) <= Variable.W.Range) || !Variable.W.IsReady())
            {
                return;
            }

            Variable.W.CastOnUnit(unit, Variable.Config.Item("usePackets").GetValue<bool>());
        }

        private static void Game_OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe)
            {
                return;
            }

            Variable.UsingE = false;
            if (spell.SData.Name != "PantheonE")
            {
                return;
            }

            Variable.UsingE = true;
            Utility.DelayAction.Add(750, () => Variable.UsingE = false);
        }

        private static void Game_OnObjectDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Pantheon_") || !sender.Name.Contains("_E_cas.troy"))
            {
                return;
            }

            Variable.UsingE = false;
        }
    }
}