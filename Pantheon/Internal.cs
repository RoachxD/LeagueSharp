#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Pantheon
{
    internal class Internal
    {
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

            if (Variable.Q.IsReady())
            {
                Variable.Q.CastOnUnit(target, Variable.Config.Item("usePackets").GetValue<bool>());
            }

            if (Variable.W.IsReady())
            {
                Variable.W.CastOnUnit(target, Variable.Config.Item("usePackets").GetValue<bool>());
            }

            if (Variable.E.IsReady() && !Variable.W.IsReady())
            {
                Variable.E.Cast(target, Variable.Config.Item("usePackets").GetValue<bool>());
            }

            if (Variable.Config.Item("comboItems").GetValue<bool>())
            {
                UseItems(target);
            }

            if (Variable.Config.Item("autoSmite").GetValue<bool>())
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

            if (!Variable.Config.Item("autoIgnite").GetValue<bool>())
            {
                return;
            }

            if (Variable.IgniteSlot == SpellSlot.Unknown ||
                Variable.Player.Spellbook.CanUseSpell(Variable.IgniteSlot) != SpellState.Ready)
            {
                return;
            }

            if (!(Variable.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) >= target.Health))
            {
                return;
            }

            Variable.Player.Spellbook.CastSpell(Variable.IgniteSlot, target);
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

            var mana = Variable.Player.MaxMana*(Variable.Config.Item("harassMana").GetValue<Slider>().Value/100.0);
            if (!(Variable.Player.Mana > mana))
            {
                return;
            }

            var menuItem = Variable.Config.Item("hMode").GetValue<StringList>().SelectedIndex;
            switch (menuItem)
            {
                case 0:
                    if (Variable.Q.IsReady())
                    {
                        Variable.Q.CastOnUnit(target, Variable.Config.Item("usePackets").GetValue<bool>());
                    }
                    break;
                case 1:
                    if (Variable.W.IsReady())
                    {
                        Variable.W.CastOnUnit(target, Variable.Config.Item("usePackets").GetValue<bool>());
                    }

                    if (!Variable.W.IsReady() && Variable.E.IsReady())
                    {
                        Variable.E.Cast(target, Variable.Config.Item("usePackets").GetValue<bool>());
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
            var mana = Variable.Player.MaxMana*(Variable.Config.Item("farmMana").GetValue<Slider>().Value/100.0);
            if (!(Variable.Player.Mana > mana))
            {
                return;
            }

            if (Variable.Config.Item("qFarm").GetValue<bool>() && Variable.Q.IsReady())
            {
                foreach (var minion in minions.Where(unit => unit.Health <= Variable.Q.GetDamage(unit)))
                {
                    Variable.Q.CastOnUnit(minion, Variable.Config.Item("usePackets").GetValue<bool>());
                    return;
                }
            }
            if (!Variable.Config.Item("wFarm").GetValue<bool>() || !Variable.W.IsReady())
            {
                return;
            }

            foreach (var minion in minions.Where(unit => unit.Health <= Variable.W.GetDamage(unit)))
            {
                Variable.W.CastOnUnit(minion, Variable.Config.Item("usePackets").GetValue<bool>());
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

            if (Variable.Config.Item("qJungle").GetValue<bool>() && Variable.Q.IsReady())
            {
                Variable.Q.CastOnUnit(mob, Variable.Config.Item("usePackets").GetValue<bool>());
            }

            if (Variable.Config.Item("wJungle").GetValue<bool>() && Variable.W.IsReady())
            {
                Variable.W.CastOnUnit(mob, Variable.Config.Item("usePackets").GetValue<bool>());
            }

            if (Variable.Config.Item("eJungle").GetValue<bool>() && Variable.E.IsReady())
            {
                Variable.E.Cast(mob, Variable.Config.Item("usePackets").GetValue<bool>());
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
                Variable.SmiteSlot = spell.Slot;
                break;
            }
        }

        public static void SetIgniteSlot()
        {
            Variable.IgniteSlot = Variable.Player.GetSpellSlot("summonerdot");
        }

        public static bool UsingEorR()
        {
            if (Variable.Player.HasBuff("Heartseeker Strike"))
            {
                Variable.UsingE = true;
            }

            return Variable.UsingE || Variable.Player.IsChannelingImportantSpell();
        }
    }
}