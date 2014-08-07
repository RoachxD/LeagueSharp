#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Pantheon___The_Artisan_of_War
{
    internal class Program
    {
        public const string CharName = "Pantheon";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;

        public static Menu Config;

        static void Main()
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != CharName) return;

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 700);

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);

            Config = new Menu(CharName, CharName, true);

            Config.AddSubMenu(new Menu("Combo Settings", "combo")); // Done
                Config.SubMenu("combo").AddItem(new MenuItem("comboKey", "Full Combo Key").SetValue(new KeyBind(32, KeyBindType.Press))); // Done
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items with Burst").SetValue(true)); // Done

            Config.AddSubMenu(new Menu("Harass Settings", "harass")); // Done
                Config.SubMenu("harass").AddItem(new MenuItem("harassKey", "Harass Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press))); // Done
                Config.SubMenu("harass").AddItem(new MenuItem("hMode", "Harass Mode: ").SetValue(new StringList(new[] { "Q", "W+E" }))); // Done
                Config.SubMenu("harass").AddItem(new MenuItem("autoQ", "Auto-Q when Target in Range").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle))); // Done
                Config.SubMenu("harass").AddItem(new MenuItem("aQT", "Don't Auto-Q if in enemy Turret Range").SetValue(true)); // Done
                Config.SubMenu("harass").AddItem(new MenuItem("harassMana", "Min. Mana Percent: ").SetValue(new Slider(50))); // Done

            Config.AddSubMenu(new Menu("Farming Settings", "farm")); // Done
                Config.SubMenu("farm").AddItem(new MenuItem("farmKey", "Farming Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press))); // Done
                Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Farm with Spear Shot (Q)").SetValue(true)); // Done
                Config.SubMenu("farm").AddItem(new MenuItem("wFarm", "Farm with Aegis of Zeonia (W)").SetValue(true)); // Done
                Config.SubMenu("farm").AddItem(new MenuItem("farmMana", "Min. Mana Percent: ").SetValue(new Slider(50))); // Done

            Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle")); // Done
                Config.SubMenu("jungle").AddItem(new MenuItem("jungleKey", "Jungle Clear Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press))); // Done
                Config.SubMenu("jungle").AddItem(new MenuItem("qJungle", "Farm with Spear Shot (Q)").SetValue(true)); // Done
                Config.SubMenu("jungle").AddItem(new MenuItem("wJungle", "Farm with Aegis of Zeonia (W)").SetValue(true)); // Done
                Config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Farm with Heartseeker Strike (E)").SetValue(true)); // Done

            Config.AddSubMenu(new Menu("Killsteal Settings", "ks")); // Done
                Config.SubMenu("ks").AddItem(new MenuItem("killSteal", "Use Smart Killsteal").SetValue(true)); // Done
                Config.SubMenu("ks").AddItem(new MenuItem("autoIgnite", "Auto Ignite").SetValue(true)); // Done

            Config.AddSubMenu(new Menu("Draw Settings", "drawing"));
                Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable All Range Draws").SetValue(false)); // Done
                Config.SubMenu("drawing").AddItem(new MenuItem("Target", "Draw Circle on Target").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 0, 0)))); // Done
                Config.SubMenu("drawing").AddItem(new MenuItem("cDraw", "Draw Damage Text").SetValue(true));
                Config.SubMenu("drawing").AddItem(new MenuItem("QDraw", "Draw Spear Shot (Q) Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 178, 0, 0)))); // Done
                Config.SubMenu("drawing").AddItem(new MenuItem("WDraw", "Draw Aegis of Zeonia (W) Range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 32, 178, 170)))); // Done
                Config.SubMenu("drawing").AddItem(new MenuItem("EDraw", "Draw Heartseeker Strike (E) Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 128, 0, 128)))); // Done

            Config.AddSubMenu(new Menu("Misc Settings", "misc")); // Done
                Config.SubMenu("misc").AddItem(new MenuItem("stopChannel", "Interrupt Channeling Spells").SetValue(true)); // Done
                Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false)); // Done

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddToMainMenu();

            Drawing.OnDraw += OnDraw;
            Game.OnGameUpdate += OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("mDraw").GetValue<bool>())
                return;

            foreach (var spell in Spells)
            {
                if (Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, Config.Item(spell.Slot + "Draw").GetValue<Circle>().Color);
            }

            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (Config.Item("Target").GetValue<Circle>().Active && target != null)
                Utility.DrawCircle(target.Position, 50, Config.Item("Target").GetValue<Circle>().Color, 1, 50);
        }

        private static void OnGameUpdate(EventArgs args)
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            var comboKey = Config.Item("comboKey").GetValue<KeyBind>().Active;
            var harassKey = Config.Item("harassKey").GetValue<KeyBind>().Active;
            var farmKey = Config.Item("farmKey").GetValue<KeyBind>().Active;
            var jungleClearKey = Config.Item("jungleKey").GetValue<KeyBind>().Active;

            Orbwalker.SetAttacks(!ObjectManager.Player.IsChanneling);
            Orbwalker.SetMovement(!ObjectManager.Player.IsChanneling);

            if (comboKey && target != null)
                Combo(target);
            else
            {
                if (harassKey && target != null)
                    Harass(target);

                if (farmKey)
                    Farm();

                if (jungleClearKey)
                    JungleClear();

                if (Config.Item("autoQ").GetValue<KeyBind>().Active && target != null)
                    if (Config.Item("aQT").GetValue<bool>() ? !Utility.UnderTurret(ObjectManager.Player, true) : Utility.UnderTurret(ObjectManager.Player, true) && ObjectManager.Player.Distance(target) <= Q.Range)
                        Q.CastOnUnit(target,  Config.Item("usePackets").GetValue<bool>());

                if (Config.Item("killSteal").GetValue<bool>())
                    Killsteal();
            }
        }

        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (Config.Item("stopChannel").GetValue<bool>())
            {
                String[] interruptingSpells = {
                    "AbsoluteZero",
                    "AlZaharNetherGrasp", 
		            "CaitlynAceintheHole", 
		            "Crowstorm", 
		            "DrainChannel", 
		            "FallenOne", 
		            "GalioIdolOfDurand", 
		            "InfiniteDuress", 
		            "KatarinaR", 
		            "MissFortuneBulletTime", 
		            "Teleport", 
		            "Pantheon_GrandSkyfall_Jump", 
		            "ShenStandUnited", 
		            "UrgotSwap2"
                };

                foreach (string interruptingSpellName in interruptingSpells)
                {
                    if (unit.Team != ObjectManager.Player.Team && args.SData.Name == interruptingSpellName)
                    {
                        if (ObjectManager.Player.Distance(unit) <= W.Range && W.IsReady())
                            W.CastOnUnit(unit, Config.Item("usePackets").GetValue<bool>());
                    }
                }
            }
        }

        private static void Combo(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (Q.IsReady())
                    Q.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                if (W.IsReady())
                    W.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                if (E.IsReady() && !target.CanMove)
                    E.Cast(target.Position, Config.Item("usePackets").GetValue<bool>());

                if (Config.Item("comboItems").GetValue<bool>())
                    UseItems(target);
            }
        }

        private static void Harass(Obj_AI_Hero target)
        {
            var mana = ObjectManager.Player.MaxMana * (Config.Item("harassMana").GetValue<Slider>().Value / 100.0);

            if (target != null && ObjectManager.Player.Mana > mana)
            {
                int menuItem = Config.Item("hMode").GetValue<StringList>().SelectedIndex;
                switch (menuItem)
                {
                    case 0:
                        Q.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                        break;
                    case 1:
                        W.CastOnUnit(target, Config.Item("usePackets").GetValue<bool>());
                        if (!target.CanMove)
                            E.Cast(target.Position, Config.Item("usePackets").GetValue<bool>());
                        break;
                }
            }
        }

        private static void Farm()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var mana = ObjectManager.Player.MaxMana * (Config.Item("farmMana").GetValue<Slider>().Value / 100.0);

            if (ObjectManager.Player.Mana > mana)
            {
                if (Config.Item("qFarm").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in minions)
                    {
                        var actualHp = (HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion) * 1000 / 1500)) <= minion.MaxHealth * 0.15) ? DamageLib.getDmg(minion, DamageLib.SpellType.Q) * 2 : DamageLib.getDmg(minion, DamageLib.SpellType.Q);
                        if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion) * 1000 / 1500)) <= actualHp)
                        {
                            Q.CastOnUnit(minion, Config.Item("usePackets").GetValue<bool>());
                            return;
                        }
                    }
                }
                if (Config.Item("wFarm").GetValue<bool>() && W.IsReady())
                {
                    foreach (var minion in minions)
                    {
                        if (minion != null && minion.IsValidTarget(W.Range) && HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion))) < DamageLib.getDmg(minion, DamageLib.SpellType.W))
                        {
                            W.CastOnUnit(minion, Config.Item("usePackets").GetValue<bool>());
                            return;
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (mob != null)
                {
                    if (Config.Item("qJungle").GetValue<bool>() && Q.IsReady())
                        Q.CastOnUnit(mob, Config.Item("usePackets").GetValue<bool>());
                    if (Config.Item("wJungle").GetValue<bool>() && W.IsReady())
                        W.CastOnUnit(mob, Config.Item("usePackets").GetValue<bool>());
                    if (Config.Item("eJungle").GetValue<bool>() && E.IsReady())
                        E.Cast(mob.Position, Config.Item("usePackets").GetValue<bool>());
                }
            }
        }

        private static void Killsteal()
        {
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team && enemy.IsValidTarget() && enemy.IsVisible))
            {
                if (enemy != null)
                {
                    var qDamage = (enemy.Health <= enemy.MaxHealth * 0.15) ? (DamageLib.getDmg(enemy, DamageLib.SpellType.Q) * 2) : DamageLib.getDmg(enemy, DamageLib.SpellType.Q);
                    var wDamage = DamageLib.getDmg(enemy, DamageLib.SpellType.W);
                    var eDamage = DamageLib.getDmg(enemy, DamageLib.SpellType.E);

                    if (enemy.Health < qDamage && Q.IsReady())
                        Q.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < wDamage && W.IsReady())
                        W.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < eDamage && E.IsReady())
                        E.Cast(enemy.Position, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < qDamage + wDamage && Q.IsReady() && W.IsReady())
                        W.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < qDamage + eDamage && Q.IsReady() && E.IsReady())
                        Q.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < wDamage + eDamage && W.IsReady() && E.IsReady())
                        W.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < qDamage + wDamage + eDamage && Q.IsReady() && W.IsReady() && E.IsReady())
                        Q.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());

                    if (Config.Item("autoIgnite").GetValue<bool>())
                    {
                        SpellDataInst ignite = ObjectManager.Player.SummonerSpellbook.Spells.FirstOrDefault(spell => spell.Name == "SummonerDot");
                        if (ignite != null && ignite.Slot != SpellSlot.Unknown && ignite.State == SpellState.Ready)
                        {
                            if (50 + ObjectManager.Player.Level * 20 >= enemy.Health)
                                ObjectManager.Player.SummonerSpellbook.CastSpell(ignite.Slot, enemy);
                        }
                    }
                }
            }
        }

        public static void UseItems(Obj_AI_Hero target)
        {
            if (target != null)
            {
                Int16[] targetedItems = { 3188, 3153, 3144, 3128, 3146, 3184 };
                Int16[] nonTargetedItems = { 3180, 3131, 3074, 3077, 3142 };

                foreach (var itemId in targetedItems)
                {
                    if (Items.HasItem(itemId) && Items.CanUseItem(itemId))
                        Items.UseItem(itemId, target);
                }

                foreach (var itemId in nonTargetedItems)
                {
                    if (Items.HasItem(itemId) && Items.CanUseItem(itemId))
                        Items.UseItem(itemId);
                }
            }
        }
    }
}