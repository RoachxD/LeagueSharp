#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Pantheon
{
    internal class Program
    {
        public const string CharName = "Pantheon";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static SpellSlot IgniteSlot;
        public static SpellSlot SmiteSlot;
        public static bool UsingE;
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (Player.BaseSkinName != CharName) return;

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 700);

            SetIgniteSlot();
            SetSmiteSlot();

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);

            Config = new Menu(CharName, CharName, true);

            Config.AddSubMenu(new Menu("Combo Settings", "combo"));
            Config.SubMenu("combo")
                .AddItem(new MenuItem("comboKey", "Full Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items with Burst").SetValue(true));
            if (SmiteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("combo")
                    .AddItem(new MenuItem("autoSmite", "Use Smite on Target if QWE Available").SetValue(true));
            }

            if (IgniteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("combo").AddItem(new MenuItem("autoIgnite", "Use Ignite with Burst").SetValue(true));
            }

            Config.AddSubMenu(new Menu("Harass Settings", "harass"));
            Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("harassKey", "Harass Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("harass")
                .AddItem(new MenuItem("hMode", "Harass Mode: ").SetValue(new StringList(new[] {"Q", "W+E"})));
            Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("autoQ", "Auto-Q when Target in Range").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.SubMenu("harass")
                .AddItem(new MenuItem("aQT", "Don't Auto-Q if in enemy Turret Range").SetValue(true));
            Config.SubMenu("harass").AddItem(new MenuItem("harassMana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            Config.AddSubMenu(new Menu("Farming Settings", "farm"));
            Config.SubMenu("farm")
                .AddItem(
                    new MenuItem("farmKey", "Farming Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Farm with Spear Shot (Q)").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("wFarm", "Farm with Aegis of Zeonia (W)").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("farmMana", "Min. Mana Percent: ").SetValue(new Slider(50)));

            Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle"));
            Config.SubMenu("jungle")
                .AddItem(
                    new MenuItem("jungleKey", "Jungle Clear Key").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));
            Config.SubMenu("jungle").AddItem(new MenuItem("qJungle", "Farm with Spear Shot (Q)").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("wJungle", "Farm with Aegis of Zeonia (W)").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Farm with Heartseeker Strike (E)").SetValue(true));

            Config.AddSubMenu(new Menu("Draw Settings", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable All Range Draws").SetValue(false));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("Target", "Draw Circle on Target").SetValue(new Circle(true,
                        Color.FromArgb(255, 255, 0, 0))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("QDraw", "Draw Spear Shot (Q) Range").SetValue(new Circle(true,
                        Color.FromArgb(255, 178, 0, 0))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("WDraw", "Draw Aegis of Zeonia (W) Range").SetValue(new Circle(false,
                        Color.FromArgb(255, 32, 178, 170))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("EDraw", "Draw Heartseeker Strike (E) Range").SetValue(new Circle(true,
                        Color.FromArgb(255, 128, 0, 128))));

            Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("stopChannel", "Interrupt Channeling Spells").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false));

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            Config.AddToMainMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
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
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var comboKey = Config.Item("comboKey").GetValue<KeyBind>().Active;
            var harassKey = Config.Item("harassKey").GetValue<KeyBind>().Active;
            var farmKey = Config.Item("farmKey").GetValue<KeyBind>().Active;
            var jungleClearKey = Config.Item("jungleKey").GetValue<KeyBind>().Active;

            Orbwalker.SetAttack(!UsingEorR());
            Orbwalker.SetMovement(!UsingEorR());

            if (comboKey && target != null)
            {
                Combo(target);
            }
            else
            {
                if (harassKey && target != null)
                {
                    Harass(target);
                }

                if (farmKey)
                {
                    Farm();
                }

                if (jungleClearKey)
                {
                    JungleClear();
                }

                if (!Config.Item("autoQ").GetValue<KeyBind>().Active || target == null)
                {
                    return;
                }

                if (Config.Item("aQT").GetValue<bool>()
                    ? !Player.UnderTurret(true)
                    : Player.UnderTurret(true) && Player.Distance(target) <= Q.Range &&
                      Q.IsReady())
                {
                    Q.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("mDraw").GetValue<bool>())
            {
                return;
            }

            foreach (var spell in Spells.Where(spell => Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active))
            {
                Utility.DrawCircle(Player.Position, spell.Range,
                    Config.Item(spell.Slot + "Draw").GetValue<Circle>().Color);
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Utility.DrawCircle(target.Position, 50, Config.Item("Target").GetValue<Circle>().Color, 1, 50);
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("stopChannel").GetValue<bool>())
            {
                return;
            }

            if (!(Player.Distance(unit) <= W.Range) || !W.IsReady())
            {
                return;
            }

            W.CastOnUnit(unit, Config.Item("usePackets").GetValue<bool>());
        }

        private static void Game_OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe)
            {
                return;
            }

            UsingE = false;
            if (spell.SData.Name.ToLower() != "pantheone")
            {
                return;
            }

            UsingE = true;
            Utility.DelayAction.Add(750, () => UsingE = false);
        }

        private static void Game_OnObjectDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Pantheon_") || !sender.Name.Contains("_E_cas.troy"))
            {
                return;
            }

            UsingE = false;
        }

        private static void Combo(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (UsingEorR())
            {
                return;
            }

            if (Q.IsReady())
            {
                Q.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
            }

            if (W.IsReady())
            {
                W.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
            }

            if (E.IsReady() && !W.IsReady())
            {
                E.Cast(target, Config.Item("usePackets").GetValue<bool>());
            }

            if (Config.Item("comboItems").GetValue<bool>())
            {
                UseItems(target);
            }

            if (Config.Item("autoSmite").GetValue<bool>())
            {
                if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                {
                    if (Q.IsReady() && W.IsReady() && E.IsReady())
                    {
                        Player.Spellbook.CastSpell(SmiteSlot, target);
                    }
                }
            }

            if (!Config.Item("autoIgnite").GetValue<bool>())
            {
                return;
            }

            if (IgniteSlot == SpellSlot.Unknown ||
                Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready)
            {
                return;
            }

            if (!(Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) >= target.Health))
            {
                return;
            }

            Player.Spellbook.CastSpell(IgniteSlot, target);
        }

        private static void Harass(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (UsingEorR())
            {
                return;
            }

            var mana = Player.MaxMana*(Config.Item("harassMana").GetValue<Slider>().Value/100.0);
            if (!(Player.Mana > mana))
            {
                return;
            }

            var menuItem = Config.Item("hMode").GetValue<StringList>().SelectedIndex;
            switch (menuItem)
            {
                case 0:
                    if (Q.IsReady())
                    {
                        Q.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                    }
                    break;
                case 1:
                    if (W.IsReady())
                    {
                        W.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                    }

                    if (!W.IsReady() && E.IsReady())
                    {
                        E.Cast(target, Config.Item("usePackets").GetValue<bool>());
                    }
                    break;
            }
        }

        private static void Farm()
        {
            if (UsingEorR())
            {
                return;
            }

            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var mana = Player.MaxMana*(Config.Item("farmMana").GetValue<Slider>().Value/100.0);
            if (!(Player.Mana > mana))
            {
                return;
            }

            if (Config.Item("qFarm").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in from minion in minions
                    let actualHp =
                        (HealthPrediction.GetHealthPrediction(minion,
                            (int) (Player.Distance(minion)*1000/1500)) <= minion.MaxHealth*0.15)
                            ? Player.GetSpellDamage(minion, SpellSlot.Q)*2
                            : Player.GetSpellDamage(minion, SpellSlot.Q)
                    where minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion,
                        (int) (Player.Distance(minion)*1000/1500)) <= actualHp
                    select minion)
                {
                    Q.CastOnUnit(minion, Config.Item("usePackets").GetValue<bool>());
                    return;
                }
            }
            if (!Config.Item("wFarm").GetValue<bool>() || !W.IsReady())
            {
                return;
            }

            foreach (
                var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(W.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int) (Player.Distance(minion))) <
                            Player.GetSpellDamage(minion, SpellSlot.W)))
            {
                W.CastOnUnit(minion, Config.Item("usePackets").GetValue<bool>());
                return;
            }
        }

        private static void JungleClear()
        {
            if (UsingEorR())
            {
                return;
            }

            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count <= 0)
            {
                return;
            }

            var mob = mobs[0];
            if (mob == null)
            {
                return;
            }

            if (Config.Item("qJungle").GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnUnit(mob, Config.Item("usePackets").GetValue<bool>());
            }

            if (Config.Item("wJungle").GetValue<bool>() && W.IsReady())
            {
                W.CastOnUnit(mob, Config.Item("usePackets").GetValue<bool>());
            }

            if (Config.Item("eJungle").GetValue<bool>() && E.IsReady())
            {
                E.Cast(mob, Config.Item("usePackets").GetValue<bool>());
            }
        }

        private static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;
            if (Q.IsReady())
            {
                dmg += (target.Health <= target.MaxHealth*0.15)
                    ? (Player.GetSpellDamage(target, SpellSlot.Q)*2)
                    : Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            return (float) dmg;
        }

        public static void UseItems(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (UsingEorR())
            {
                return;
            }

            Int16[] targetedItems = {3188, 3153, 3144, 3128, 3146, 3184};
            Int16[] nonTargetedItems = {3180, 3131, 3074, 3077, 3142};

            foreach (var itemId in targetedItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
            {
                Items.UseItem(itemId, target);
            }

            foreach (var itemId in nonTargetedItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
            {
                Items.UseItem(itemId);
            }
        }

        public static string SmiteType()
        {
            int[] redSmite = {3715, 3718, 3717, 3716, 3714};
            int[] blueSmite = {3706, 3710, 3709, 3708, 3707};

            return blueSmite.Any(itemId => Items.HasItem(itemId))
                ? "s5_summonersmiteplayerganker"
                : (redSmite.Any(itemId => Items.HasItem(itemId)) ? "s5_summonersmiteduel" : "summonersmite");
        }

        public static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, SmiteType(), StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
                break;
            }
        }

        public static void SetIgniteSlot()
        {
            IgniteSlot = Player.GetSpellSlot("summonerdot");
        }

        public static bool UsingEorR()
        {
            if (Player.HasBuff("pantheonesound"))
            {
                UsingE = true;
            }

            return UsingE || Player.IsChannelingImportantSpell();
        }
    }
}