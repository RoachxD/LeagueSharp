#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

using Color = System.Drawing.Color;

namespace Kayle
{
    class Program
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

        private static Obj_AI_Hero _player;

        public static Menu Config;

        private static bool RighteousFuryActive
        {
            get { return ObjectManager.Player.AttackRange > 125f; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;

            if (_player.ChampionName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 650f);
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 625f);
            R = new Spell(SpellSlot.R, 900f);

            IgniteSlot = _player.GetSpellSlot("SummonerDot");

            Dfg = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu(ChampionName, ChampionName, true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var tsMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W").SetValue(false));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseIgniteC", "Use Ignite").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(
                    new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
                Config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
                Config.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W").SetValue(false));
                Config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E").SetValue(false));
                Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(
                    new KeyBind(Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
                Config.SubMenu("Harass").AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(
                    new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
                Config.SubMenu("Farm").AddItem(new MenuItem("UseQF", "Use Q").SetValue(
                    new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 1)));
                Config.SubMenu("Farm").AddItem(new MenuItem("UseEF", "Use E").SetValue(
                    new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 2)));
                Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(
                    new KeyBind(Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
                Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(
                    new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Jungle Farm", "JungleFarm"));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJ", "Use Q").SetValue(true));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJ", "Use E").SetValue(true));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(
                    new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Ultimate", "Ultimate"));
                Config.SubMenu("Ultimate").AddSubMenu(new Menu("Allies", "Allies"));
                    foreach (var ally in ObjectManager.Get<Obj_AI_Hero>()
                        .Where(ally => ally.IsAlly))
                            Config.SubMenu("Ultimate").SubMenu("Allies").AddItem(new MenuItem("Ult" + ally.ChampionName, ally.ChampionName)
                                .SetValue(ally.ChampionName == _player.ChampionName));
                Config.SubMenu("Ultimate").AddItem(new MenuItem("UltMinHP", "Min Percentage of HP").SetValue(new Slider(20, 1)));

            Config.AddSubMenu(new Menu("Heal", "Heal"));
                Config.SubMenu("Heal").AddSubMenu(new Menu("Allies", "Allies"));
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => ally.IsAlly))
                    Config.SubMenu("Heal").SubMenu("Allies").AddItem(new MenuItem("Heal" + ally.ChampionName, ally.ChampionName)
                        .SetValue(ally.ChampionName == _player.ChampionName));
                Config.SubMenu("Heal").AddItem(new MenuItem("HealMinHP", "Min Percentage of HP").SetValue(new Slider(40, 1)));
                

            Config.AddSubMenu(new Menu("Misc", "Misc"));
                Config.SubMenu("Misc").AddItem(new MenuItem("UsePackets", "Use Packets").SetValue(true));
                Config.SubMenu("Misc").AddItem(new MenuItem("SupportMode", "Support Mode").SetValue(false));

                var comboDmg = new MenuItem("ComboDamage", "Draw damage after combo").SetValue(true);
                Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
                Utility.HpBarDamageIndicator.Enabled = comboDmg.GetValue<bool>();
                comboDmg.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

                Config.AddSubMenu(new Menu("Drawings", "Drawings"));
                    Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(
                        new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                    Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(
                        new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                    Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(
                        new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                    Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(
                        new Circle(false, Color.FromArgb(100, 255, 0, 255))));
                    Config.SubMenu("Drawings").AddItem(comboDmg);

                Config.AddToMainMenu();

            Game.PrintChat("<font color=\"#00BFFF\">Kayle# -</font> <font color=\"#FFFFFF\">Loaded</font>");

            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Combo()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var iTarget = SimpleTs.GetTarget(600,     SimpleTs.DamageType.True);

            if (qTarget == null && wTarget == null && eTarget == null && iTarget == null)
                return;

            if (Config.Item("UseQC").GetValue<bool>() && Q.IsReady() && qTarget != null)
                Q.Cast(qTarget, Config.Item("UsePackets").GetValue<bool>());

            if (Config.Item("UseWC").GetValue<bool>() && W.IsReady() && wTarget != null && _player.Distance(wTarget) >= Orbwalking.GetRealAutoAttackRange(_player))
                W.Cast(_player, Config.Item("UsePackets").GetValue<bool>());

            if (Config.Item("UseEC").GetValue<bool>() && E.IsReady() && eTarget != null && _player.Distance(eTarget) <= E.Range)
                E.Cast();

            if (iTarget != null && Config.Item("UseIgniteC").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                _player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (_player.GetSummonerSpellDamage(iTarget, Damage.SummonerSpell.Ignite) > iTarget.Health)
                    _player.SummonerSpellbook.CastSpell(IgniteSlot, iTarget);
            }

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>()
                .Where(minion => minion.Distance(eTarget) <= 150 && eTarget != null && RighteousFuryActive && !Config.Item("SupportMode").GetValue<bool>()))
                    Orbwalker.ForceTarget(minion);
        }

        private static void Harass()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);

            if (qTarget == null && eTarget == null)
                return;

            if (Config.Item("UseQH").GetValue<bool>() && Q.IsReady() && qTarget != null)
                Q.Cast(qTarget, Config.Item("UsePackets").GetValue<bool>());

            if (Config.Item("UseWH").GetValue<bool>() && W.IsReady() && wTarget != null && _player.Distance(wTarget) >= Orbwalking.GetRealAutoAttackRange(_player))
                W.Cast(_player, Config.Item("UsePackets").GetValue<bool>());

            if (Config.Item("UseEH").GetValue<bool>() && E.IsReady() && eTarget != null && _player.Distance(eTarget) <= E.Range)
                E.Cast();

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>()
                .Where(minion => minion.Distance(eTarget) <= 150 && eTarget != null && RighteousFuryActive && !Config.Item("SupportMode").GetValue<bool>()))
                Orbwalker.ForceTarget(minion);
        }

        private static void Farm(bool laneClear)
        {
            var allMinionsQ = MinionManager.GetMinions(_player.ServerPosition, Q.Range);
            var allMinionsE = MinionManager.GetMinions(_player.ServerPosition, E.Range + 150);

            var useQi = Config.Item("UseQF").GetValue<StringList>().SelectedIndex;
            var useEi = Config.Item("UseEF").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && Q.IsReady())
                foreach (var minion in allMinionsQ.Where(minion => !Orbwalking.InAutoAttackRange(minion) &&
                    minion.Health < _player.GetSpellDamage(minion, SpellSlot.Q)))
                        Q.Cast(minion, Config.Item("UsePackets").GetValue<bool>());

            if (useE && E.IsReady())
            {
                if (laneClear)
                {
                    foreach (var minion in allMinionsE.Where(minion => _player.Distance(minion) <= E.Range))
                        E.Cast();

                    foreach (var minion in allMinionsE
                        .Where(eMinion => _player.Distance(eMinion) > E.Range && _player.Distance(eMinion) <= E.Range + 150)
                            .SelectMany(eMinion => ObjectManager.Get<Obj_AI_Minion>()
                                .Where(minion => eMinion.Distance(minion) <= 150 && eMinion != minion)))
                                    Orbwalker.ForceTarget(minion);
                }
                else
                {
                    foreach (var minion in allMinionsE
                        .Where(minion => _player.Distance(minion) > Orbwalking.GetRealAutoAttackRange(_player) && !RighteousFuryActive))
                            E.Cast();
                }
            }
        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("UseQJ").GetValue<bool>();
            var useE = Config.Item("UseEJ").GetValue<bool>();

            var mobs = MinionManager.GetMinions(_player.ServerPosition, W.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            var mob = mobs[0];

            if (Q.IsReady() && useQ)
                Q.Cast(mob, Config.Item("UsePackets").GetValue<bool>());

            if (useE && E.IsReady())
                E.Cast();
        }

        private static void Ultimate()
        {
            foreach (var ally in from ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(ally => ally.IsAlly && !ally.IsDead && ally.HasBuffOfType(BuffType.Damage))
                    let menuItem = Config.Item("Ult" + ally.ChampionName).GetValue<bool>()
                        where menuItem && Config.Item("UltMinHP").GetValue<Slider>().Value < (ally.Health/ally.MaxHealth) * 100
                            select ally)
                R.Cast(ally, Config.Item("UsePackets").GetValue<bool>());
        }

        private static void Heal()
        {
            foreach (var ally in from ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(ally => ally.IsAlly && !ally.IsDead)
                    let menuItem = Config.Item("Heal" + ally.ChampionName).GetValue<bool>()
                        where menuItem && Config.Item("HealMinHP").GetValue<Slider>().Value < (ally.Health / ally.MaxHealth) * 100
                            select ally)
                W.Cast(ally, Config.Item("UsePackets").GetValue<bool>());
        }

        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q);

            if (Dfg.IsReady())
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (E.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && _player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage * (Dfg.IsReady() ? 1.2f : 1);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_player.IsDead) return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active ||
                    Config.Item("HarassActiveT").GetValue<KeyBind>().Active)
                        Harass();

                var laneClear = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if ((laneClear || Config.Item("FreezeActive").GetValue<KeyBind>().Active) && !Config.Item("SupportMode").GetValue<bool>())
                    Farm(laneClear);

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }

            Ultimate();
            Heal();
        }
        
        private static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != Packet.C2S.Move.Header)
                return;

            var decodedPacket = Packet.C2S.Move.Decoded(args.PacketData);
            if (decodedPacket.MoveType == 3 && (Orbwalker.GetTarget().IsMinion && !Config.Item("SupportMode").GetValue<bool>()))
                args.Process = false;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(_player.Position, spell.Range, menuItem.Color);
            }

            var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var eDamage = 20 + ((E.Level - 1) * 10) + (_player.BaseAbilityDamage * 0.25);
            if (Config.Item("ComboDamage").GetValue<bool>())
                Drawing.DrawText(target.ServerPosition.X, target.ServerPosition.Y, Color.White, ((target.Health - ComboDamage(target)) / (RighteousFuryActive ? (eDamage) : (_player.GetAutoAttackDamage(target)))).ToString(CultureInfo.InvariantCulture));
        }
    }
}