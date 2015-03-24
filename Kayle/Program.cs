#region

using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Kayle
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += delegate
                {
                    var onGameLoad = new Thread(Game_OnGameLoad);
                    onGameLoad.Start();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Game_OnGameLoad()
        {
            if (Variable.Player.ChampionName != Variable.ChampionName) return;

            Variable.Q = new Spell(SpellSlot.Q, 650f);
            Variable.W = new Spell(SpellSlot.W, 900f);
            Variable.E = new Spell(SpellSlot.E, 625f);
            Variable.R = new Spell(SpellSlot.R, 900f);

            Variable.IgniteSlot = Variable.Player.GetSpellSlot("summonerdot");

            Variable.Dfg = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline
                ? new Items.Item(3188, 750)
                : new Items.Item(3128, 750);

            Variable.SpellList.Add(Variable.Q);
            Variable.SpellList.Add(Variable.W);
            Variable.SpellList.Add(Variable.E);
            Variable.SpellList.Add(Variable.R);

            Variable.Config = new Menu(Variable.ChampionName, Variable.ChampionName, true);

            Variable.Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Variable.Config.AddSubMenu(tsMenu);

            Variable.Orbwalker = new Orbwalking.Orbwalker(Variable.Config.SubMenu("Orbwalking"));

            Variable.Config.AddSubMenu(new Menu("Combo", "Combo"));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W").SetValue(false));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("UseIgniteC", "Use Ignite").SetValue(true));
            Variable.Config.SubMenu("Combo")
                .AddItem(new MenuItem("CMinions", "Attack minions if Target out of Range").SetValue(true));
            Variable.Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(
                new KeyBind(Variable.Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Variable.Config.AddSubMenu(new Menu("Harass", "Harass"));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W").SetValue(false));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E").SetValue(false));
            Variable.Config.SubMenu("Harass")
                .AddItem(new MenuItem("HMinions", "Attack minions if Target out of Range").SetValue(true));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(
                new KeyBind(Variable.Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Variable.Config.SubMenu("Harass").AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(
                new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

            Variable.Config.AddSubMenu(new Menu("Farm", "Farm"));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("UseQF", "Use Q").SetValue(
                new StringList(new[] {"Freeze", "LaneClear", "Both", "No"}, 1)));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("UseEF", "Use E").SetValue(
                new StringList(new[] {"Freeze", "LaneClear", "Both", "No"}, 2)));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(
                new KeyBind(Variable.Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Variable.Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "Lane Clear!").SetValue(
                new KeyBind(Variable.Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Variable.Config.AddSubMenu(new Menu("Jungle Farm", "JungleFarm"));
            Variable.Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJ", "Use Q").SetValue(true));
            Variable.Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJ", "Use E").SetValue(true));
            Variable.Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "Jungle Farm!").SetValue(
                new KeyBind(Variable.Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Variable.Config.AddSubMenu(new Menu("Ultimate", "Ultimate"));
            Variable.Config.SubMenu("Ultimate").AddSubMenu(new Menu("Allies", "Allies"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(ally => ally.IsAlly))
                Variable.Config.SubMenu("Ultimate")
                    .SubMenu("Allies")
                    .AddItem(new MenuItem("Ult" + ally.ChampionName, ally.ChampionName)
                        .SetValue(ally.ChampionName == Variable.Player.ChampionName));
            Variable.Config.SubMenu("Ultimate")
                .AddItem(new MenuItem("UltMinHP", "Min Percentage of HP").SetValue(new Slider(20, 1)));

            Variable.Config.AddSubMenu(new Menu("Heal", "Heal"));
            Variable.Config.SubMenu("Heal").AddSubMenu(new Menu("Allies", "Allies"));
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(ally => ally.IsAlly))
            {
                Variable.Config.SubMenu("Heal")
                    .SubMenu("Allies")
                    .AddItem(new MenuItem("Heal" + ally.ChampionName, ally.ChampionName)
                        .SetValue(ally.ChampionName == Variable.Player.ChampionName));
            }
            Variable.Config.SubMenu("Heal")
                .AddItem(new MenuItem("HealMinHP", "Min Percentage of HP").SetValue(new Slider(40, 1)));


            Variable.Config.AddSubMenu(new Menu("Misc", "Misc"));
            Variable.Config.SubMenu("Misc").AddItem(new MenuItem("UsePackets", "Use Packets").SetValue(true));
            Variable.Config.SubMenu("Misc").AddItem(new MenuItem("SupportMode", "Support Mode").SetValue(false));

            var comboDmg = new MenuItem("ComboDamage", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = Internal.ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = comboDmg.GetValue<bool>();
            comboDmg.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            Variable.Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(
                new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(
                new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(
                new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Variable.Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(
                new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Variable.Config.SubMenu("Drawings").AddItem(comboDmg);

            Variable.Config.AddToMainMenu();

            Game.PrintChat("<font color=\"#00BFFF\">Kayle# -</font> <font color=\"#FFFFFF\">Loaded</font>");

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Variable.Player.IsDead)
            {
                return;
            }

            if ((((Obj_AI_Base) Variable.Orbwalker.GetTarget()).IsMinion))
            {
                Variable.Orbwalker.SetAttack(!Variable.Config.SubMenu("Misc").Item("SupportMode").GetValue<bool>());
            }

            if (Variable.Config.SubMenu("Combo").Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Internal.Combo();
            }
            else
            {
                if (Variable.Config.SubMenu("Harass").Item("HarassActive").GetValue<KeyBind>().Active ||
                    Variable.Config.SubMenu("Harass").Item("HarassActiveT").GetValue<KeyBind>().Active)
                {
                    Internal.Harass();
                }

                var laneClear = Variable.Config.SubMenu("Farm").Item("LaneClearActive").GetValue<KeyBind>().Active;
                if ((laneClear || Variable.Config.SubMenu("Farm").Item("FreezeActive").GetValue<KeyBind>().Active) &&
                    !Variable.Config.SubMenu("Misc").Item("SupportMode").GetValue<bool>())
                {
                    Internal.Farm(laneClear);
                }

                if (Variable.Config.SubMenu("JungleFarm").Item("JungleFarmActive").GetValue<KeyBind>().Active)
                {
                    Internal.JungleFarm();
                }
            }

            Internal.Ultimate();
            Internal.Heal();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Internal.DrawCircles();

            var target = TargetSelector.GetTarget(Variable.W.Range, TargetSelector.DamageType.Magical);
            var eDamage = 20 + ((Variable.E.Level - 1)*10) + (Variable.Player.BaseAbilityDamage*0.25);
            if (Variable.Config.SubMenu("Drawings").Item("ComboDamage").GetValue<bool>())
            {
                Drawing.DrawText(target.Position.X, target.Position.Y, Color.White,
                    ((target.Health - Internal.ComboDamage(target))/
                     (Variable.RighteousFuryActive ? (eDamage) : (Variable.Player.GetAutoAttackDamage(target))))
                        .ToString(
                            CultureInfo.InvariantCulture));
            }
        }
    }
}