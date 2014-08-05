#region

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

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

        static void Main(string[] args)
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
                Config.SubMenu("harass").AddItem(new MenuItem("hMode", "Harass Mode: ").SetValue(new StringList(new[] { "Q", "W+E" }, 0))); // Done
                Config.SubMenu("harass").AddItem(new MenuItem("autoQ", "Auto-Q when Target in Range").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle))); // Done
                Config.SubMenu("harass").AddItem(new MenuItem("aQT", "Don't Auto-Q if in enemy Turret Range").SetValue(true)); // Done
                Config.SubMenu("harass").AddItem(new MenuItem("harassMana", "Min. Mana Percent: ").SetValue(new Slider(50, 100, 0))); // Done

            Config.AddSubMenu(new Menu("Farming Settings", "farm")); // Done
                Config.SubMenu("farm").AddItem(new MenuItem("farmKey", "Farming Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press))); // Done
                Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Farm with Spear Shot (Q)").SetValue(true)); // Done
                Config.SubMenu("farm").AddItem(new MenuItem("wFarm", "Farm with Aegis of Zeonia (W)").SetValue(true)); // Done
                Config.SubMenu("farm").AddItem(new MenuItem("farmMana", "Min. Mana Percent: ").SetValue(new Slider(50, 100, 0))); // Done

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

            var Target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (Config.Item("Target").GetValue<Circle>().Active && Target != null)
                Utility.DrawCircle(Target.Position, 50, Config.Item("Target").GetValue<Circle>().Color, 1, 50);
        }

        private static void OnGameUpdate(EventArgs args)
        {
            var Target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            var ComboKey = Config.Item("comboKey").GetValue<KeyBind>().Active;
            var HarassKey = Config.Item("harassKey").GetValue<KeyBind>().Active;
            var FarmKey = Config.Item("farmKey").GetValue<KeyBind>().Active;
            var JungleClearKey = Config.Item("jungleKey").GetValue<KeyBind>().Active;

            Orbwalker.SetAttacks(!ObjectManager.Player.IsChanneling ? true : false);
            Orbwalker.SetMovement(!ObjectManager.Player.IsChanneling ? true : false);

            if (ComboKey && Target != null)
                Combo(Target);
            else
            {
                if (HarassKey && Target != null)
                    Harass(Target);

                if (FarmKey)
                    Farm();

                if (JungleClearKey)
                    JungleClear();

                if (Config.Item("autoQ").GetValue<KeyBind>().Active && Target != null)
                    if (Config.Item("aQT").GetValue<bool>() ? !Utility.UnderTurret(ObjectManager.Player, true) : Utility.UnderTurret(ObjectManager.Player, true) && ObjectManager.Player.Distance(Target) <= Q.Range)
                        Q.CastOnUnit(Target,  Config.Item("usePackets").GetValue<bool>());

                if (Config.Item("killSteal").GetValue<bool>())
                    Killsteal();
            }
        }

        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (Config.Item("stopChannel").GetValue<bool>())
            {
                String[] InterruptingSpells = {
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

                foreach (string InterruptingSpellName in InterruptingSpells)
                {
                    if (unit.Team != ObjectManager.Player.Team && args.SData.Name == InterruptingSpellName && unit != null)
                    {
                        if (ObjectManager.Player.Distance(unit) <= W.Range && W.IsReady())
                            W.CastOnUnit(unit, Config.Item("usePackets").GetValue<bool>());
                    }
                }
            }
        }

        private static void Combo(Obj_AI_Hero Target)
        {
            if (Target != null)
            {
                if (Q.IsReady())
                    Q.CastOnUnit(Target, Config.Item("usePackets").GetValue<bool>());
                if (W.IsReady())
                    W.CastOnUnit(Target, Config.Item("usePackets").GetValue<bool>());
                if (E.IsReady() && !Target.CanMove)
                    E.Cast(Target.Position, Config.Item("usePackets").GetValue<bool>());

                if (Config.Item("comboItems").GetValue<bool>())
                    UseItems(Target);
            }
        }

        private static void Harass(Obj_AI_Hero Target)
        {
            var Mana = ObjectManager.Player.MaxMana * (Config.Item("harassMana").GetValue<Slider>().Value / 100);

            if (Target != null && ObjectManager.Player.Mana > Mana)
            {
                int MenuItem = Config.Item("hMode").GetValue<StringList>().SelectedIndex;
                switch (MenuItem)
                {
                    case 0:
                        Q.CastOnUnit(Target, Config.Item("usePackets").GetValue<bool>());
                        break;
                    case 1:
                        W.CastOnUnit(Target, Config.Item("usePackets").GetValue<bool>());
                        if (!Target.CanMove)
                            E.Cast(Target.Position, Config.Item("usePackets").GetValue<bool>());
                        break;
                }
            }
        }

        private static void Farm()
        {
            var Minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var Mana = ObjectManager.Player.MaxMana * (Config.Item("farmMana").GetValue<Slider>().Value / 100);

            if (ObjectManager.Player.Mana > Mana)
            {
                if (Config.Item("qFarm").GetValue<bool>() && Q.IsReady())
                {
                    foreach (var minion in Minions)
                    {
                        var Actual_HP = (HealthPrediction.GetHealthPrediction(minion, (int)(Geometry.Distance(ObjectManager.Player, minion) * 1000 / 1500)) <= minion.MaxHealth * 0.15) ? DamageLib.getDmg(minion, DamageLib.SpellType.Q) * 2 : DamageLib.getDmg(minion, DamageLib.SpellType.Q);
                        if (minion != null && minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Geometry.Distance(ObjectManager.Player, minion) * 1000 / 1500)) <= Actual_HP)
                        {
                            Q.CastOnUnit(minion, Config.Item("usePackets").GetValue<bool>());
                            return;
                        }
                    }
                }
                if (Config.Item("wFarm").GetValue<bool>() && W.IsReady())
                {
                    foreach (var minion in Minions)
                    {
                        if (minion != null && minion.IsValidTarget(W.Range) && HealthPrediction.GetHealthPrediction(minion, (int)(Geometry.Distance(ObjectManager.Player, minion))) < DamageLib.getDmg(minion, DamageLib.SpellType.W))
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
            var Mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count > 0)
            {
                var mob = Mobs[0];
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
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team && Utility.IsValidTarget(enemy) && enemy.IsVisible))
            {
                if (enemy != null)
                {
                    var Q_Damage = (enemy.Health <= enemy.MaxHealth * 0.15) ? (DamageLib.getDmg(enemy, DamageLib.SpellType.Q) * 2) : DamageLib.getDmg(enemy, DamageLib.SpellType.Q); ;
                    var W_Damage = DamageLib.getDmg(enemy, DamageLib.SpellType.W);
                    var E_Damage = DamageLib.getDmg(enemy, DamageLib.SpellType.E);

                    if (enemy.Health < Q_Damage && Q.IsReady())
                        Q.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < W_Damage && W.IsReady())
                        W.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < E_Damage && E.IsReady())
                        E.Cast(enemy.Position, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < Q_Damage + W_Damage && Q.IsReady() && W.IsReady())
                        W.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < Q_Damage + E_Damage && Q.IsReady() && E.IsReady())
                        Q.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < W_Damage + E_Damage && W.IsReady() && E.IsReady())
                        W.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());
                    else if (enemy.Health < Q_Damage + W_Damage + E_Damage && Q.IsReady() && W.IsReady() && E.IsReady())
                        Q.CastOnUnit(enemy, Config.Item("usePackets").GetValue<bool>());

                    if (Config.Item("autoIgnite").GetValue<bool>())
                    {
                        SpellDataInst Ignite = ObjectManager.Player.SummonerSpellbook.Spells.FirstOrDefault(spell => spell.Name == "SummonerDot");
                        if (Ignite != null && Ignite.Slot != SpellSlot.Unknown && Ignite.State == SpellState.Ready)
                        {
                            if (50 + ObjectManager.Player.Level * 20 >= enemy.Health)
                                ObjectManager.Player.SummonerSpellbook.CastSpell(Ignite.Slot, enemy);
                        }
                    }
                }
            }
        }

        public static void UseItems(Obj_AI_Hero Target)
        {
            if (Target != null)
            {
                Int16[] TargetedItems = { 3188, 3153, 3144, 3128, 3146, 3184 };
                Int16[] NonTargetedItems = { 3180, 3131, 3074, 3077, 3142 };

                foreach (int ItemID in TargetedItems)
                {
                    if (Items.HasItem(ItemID) && Items.CanUseItem(ItemID))
                        Items.UseItem(ItemID, Target);
                }

                foreach (int ItemID in NonTargetedItems)
                {
                    if (Items.HasItem(ItemID) && Items.CanUseItem(ItemID))
                        Items.UseItem(ItemID);
                }
            }
        }
    }
}