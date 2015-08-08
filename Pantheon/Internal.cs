using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Pantheon
{
    internal class Internal
    {
        private static int _lastTick;

        public static void Combo(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (UsingEorR())
            {
                return;
            }

            if (Variable.Config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex == 0)
            {
                if (Variable.Q.IsReady())
                {
                    Variable.Q.CastOnUnit(target, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                }

                if (Variable.W.IsReady())
                {
                    Variable.W.CastOnUnit(target, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                }

                if (Variable.E.IsReady() && !Variable.W.IsReady())
                {
                    Variable.E.Cast(target, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                }
            }
            else
            {
                if (Variable.Q.IsReady() && !Variable.W.IsReady())
                {
                    Variable.Q.CastOnUnit(target, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                }

                if (Variable.W.IsReady())
                {
                    Variable.W.CastOnUnit(target, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                }

                if (Variable.E.IsReady() && !target.CanMove)
                {
                    Variable.E.Cast(target, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                }
            }

            if (Variable.Config.SubMenu("Combo").Item("ComboItems").GetValue<bool>())
            {
                UseItems(target);
            }

            if (Variable.Config.SubMenu("Combo").Item("AutoSmite").GetValue<bool>())
            {
                if (Variable.SmiteSlot != SpellSlot.Unknown &&
                    Variable.Player.Spellbook.CanUseSpell(Variable.SmiteSlot) == SpellState.Ready)
                {
                    if (Variable.Q.IsReady() && Variable.W.IsReady() && Variable.E.IsReady())
                    {
                        Variable.Player.Spellbook.CastSpell(Variable.SmiteSlot, target);
                    }
                }
            }

            if (Variable.IgniteSlot == SpellSlot.Unknown ||
                Variable.Player.Spellbook.CanUseSpell(Variable.IgniteSlot) != SpellState.Ready)
            {
                return;
            }

            if (Variable.Config.SubMenu("Combo").Item("AutoIgnite").GetValue<StringList>().SelectedIndex == 1)
            {
                if (Variable.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) >= target.Health)
                {
                    Variable.Player.Spellbook.CastSpell(Variable.IgniteSlot, target);
                }
            }
            else
            {
                Variable.Player.Spellbook.CastSpell(Variable.IgniteSlot, target);
            }
        }

        public static void Harass(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (UsingEorR())
            {
                return;
            }

            var mana = Variable.Player.MaxMana*
                       (Variable.Config.SubMenu("Harass").Item("HarassMana").GetValue<Slider>().Value/100.0);
            if (!(Variable.Player.Mana > mana))
            {
                return;
            }

            var menuItem = Variable.Config.SubMenu("Harass").Item("HarassMode").GetValue<StringList>().SelectedIndex;
            switch (menuItem)
            {
                case 0:
                    if (Variable.Q.IsReady())
                    {
                        Variable.Q.CastOnUnit(target,
                            Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                    }
                    break;
                case 1:
                    if (Variable.W.IsReady())
                    {
                        Variable.W.CastOnUnit(target,
                            Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                    }

                    if (!Variable.W.IsReady() && Variable.E.IsReady())
                    {
                        Variable.E.Cast(target, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                    }
                    break;
            }
        }

        public static void Farm()
        {
            if (UsingEorR())
            {
                return;
            }

            var minions = MinionManager.GetMinions(Variable.Player.ServerPosition, Variable.Q.Range);
            var mana = Variable.Player.MaxMana*
                       (Variable.Config.SubMenu("Farm").Item("FarmMana").GetValue<Slider>().Value/100.0);
            if (!(Variable.Player.Mana > mana))
            {
                return;
            }

            if (Variable.Config.SubMenu("Farm").Item("FarmQ").GetValue<bool>() && Variable.Q.IsReady())
            {
                foreach (var minion in minions.Where(unit => unit.Health <= Variable.Q.GetDamage(unit)))
                {
                    Variable.Q.CastOnUnit(minion, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                    return;
                }
            }
            if (!Variable.Config.SubMenu("Farm").Item("FarmW").GetValue<bool>() || !Variable.W.IsReady())
            {
                return;
            }

            foreach (var minion in minions.Where(unit => unit.Health <= Variable.W.GetDamage(unit)))
            {
                Variable.W.CastOnUnit(minion, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
                return;
            }
        }

        public static void JungleClear()
        {
            if (UsingEorR())
            {
                return;
            }

            var mobs = MinionManager.GetMinions(Variable.Player.ServerPosition, Variable.Q.Range, MinionTypes.All,
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

            if (Variable.Config.SubMenu("Jungle").Item("JungleQ").GetValue<bool>() && Variable.Q.IsReady())
            {
                Variable.Q.CastOnUnit(mob, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
            }

            if (Variable.Config.SubMenu("Jungle").Item("JungleW").GetValue<bool>() && Variable.W.IsReady())
            {
                Variable.W.CastOnUnit(mob, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
            }

            if (Variable.Config.SubMenu("Jungle").Item("JungleE").GetValue<bool>() && Variable.E.IsReady())
            {
                Variable.E.Cast(mob, Variable.Config.SubMenu("Misc").Item("UsePackets").GetValue<bool>());
            }
        }

        public static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;
            if (Variable.Q.IsReady())
            {
                dmg += (target.Health <= target.MaxHealth*0.15)
                    ? (Variable.Player.GetSpellDamage(target, SpellSlot.Q)*2)
                    : Variable.Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (Variable.W.IsReady())
            {
                dmg += Variable.Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (Variable.E.IsReady())
            {
                dmg += Variable.Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (Variable.IgniteSlot != SpellSlot.Unknown &&
                Variable.Player.Spellbook.CanUseSpell(Variable.IgniteSlot) == SpellState.Ready)
            {
                dmg += Variable.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
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

            short[] targetedItems = {3188, 3153, 3144, 3128, 3146, 3184};
            short[] nonTargetedItems = {3180, 3131, 3074, 3077, 3142};

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
                        spell => string.Equals(spell.Name, SmiteType(), StringComparison.CurrentCultureIgnoreCase)))
            {
                Variable.SmiteSlot = spell.Slot;
                break;
            }
        }

        public static void SetIgniteSlot()
        {
            Variable.IgniteSlot = Variable.Player.GetSpellSlot("summonerdot");
        }

        public static void ComboModeSwitch()
        {
            var comboMode = Variable.Config.SubMenu("Combo").Item("ComboMode").GetValue<StringList>().SelectedIndex;
            var lasttime = Environment.TickCount - _lastTick;
            if (!Variable.Config.SubMenu("Combo").Item("ComboSwitch").GetValue<KeyBind>().Active ||
                lasttime <= Game.Ping)
            {
                return;
            }

            switch (comboMode)
            {
                case 0:
                    Variable.Config.SubMenu("Combo").Item("ComboMode")
                        .SetValue(
                            new StringList(
                                new[]
                                {
                                    "Normal (Q-W-E with No Restrictions)",
                                    "Ganking (W-E-Q - Will not E untill target stunned)"
                                }, 1));
                    _lastTick = Environment.TickCount + 300;
                    break;
                case 1:
                    Variable.Config.SubMenu("Combo").Item("ComboMode")
                        .SetValue(
                            new StringList(
                                new[]
                                {
                                    "Normal (Q-W-E with No Restrictions)",
                                    "Ganking (W-E-Q - Will not E untill target stunned)"
                                }));
                    _lastTick = Environment.TickCount + 300;
                    break;
            }
        }

        public static bool UsingEorR()
        {
            return Variable.Player.IsChannelingImportantSpell();
        }
    }
}