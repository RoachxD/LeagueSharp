#region

using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Kayle
{
    internal class Function
    {
        public static void Combo()
        {
            var qTarget = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(Variable.W.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(Variable.E.Range, TargetSelector.DamageType.Magical);
            var iTarget = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);

            if (qTarget == null && wTarget == null && eTarget == null && iTarget == null)
            {
                return;
            }

            if (Variable.Config.Item("UseQC").GetValue<bool>() && Variable.Q.IsReady() && qTarget != null)
            {
                Variable.Q.Cast(qTarget, Variable.Config.Item("UsePackets").GetValue<bool>());
            }

            if (Variable.Config.Item("UseWC").GetValue<bool>() && Variable.W.IsReady() && wTarget != null &&
                !Orbwalking.InAutoAttackRange(wTarget))
            {
                Variable.W.Cast(Variable.Player, Variable.Config.Item("UsePackets").GetValue<bool>());
            }

            if (Variable.Config.Item("UseEC").GetValue<bool>() && Variable.E.IsReady() && eTarget != null &&
                Variable.Player.Distance(eTarget) <= Variable.E.Range)
            {
                Variable.E.Cast();
            }

            if (iTarget != null && Variable.Config.Item("UseIgniteC").GetValue<bool>() &&
                Variable.IgniteSlot != SpellSlot.Unknown &&
                Variable.Player.Spellbook.CanUseSpell(Variable.IgniteSlot) == SpellState.Ready)
            {
                if (Variable.Player.GetSummonerSpellDamage(iTarget, Damage.SummonerSpell.Ignite) > iTarget.Health)
                {
                    Variable.Player.Spellbook.CastSpell(Variable.IgniteSlot, iTarget);
                }
            }

            if (!Variable.Config.Item("CMinions").GetValue<bool>() || Variable.Config.Item("SupportMode").GetValue<bool>())
            {
                return;
            }

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>()
                .Where(
                    minion =>
                        minion.Distance(eTarget) <= 150 && !Orbwalking.InAutoAttackRange(eTarget) && eTarget != null))
            {
                Variable.Orbwalker.ForceTarget(minion);
            }
        }

        public static void Harass()
        {
            var qTarget = TargetSelector.GetTarget(Variable.Q.Range, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(Variable.W.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(Variable.E.Range, TargetSelector.DamageType.Magical);

            if (qTarget == null && eTarget == null)
            {
                return;
            }

            if (Variable.Config.Item("UseQH").GetValue<bool>() && Variable.Q.IsReady() && qTarget != null)
            {
                Variable.Q.Cast(qTarget, Variable.Config.Item("UsePackets").GetValue<bool>());
            }

            if (Variable.Config.Item("UseWH").GetValue<bool>() && Variable.W.IsReady() && wTarget != null &&
                !Orbwalking.InAutoAttackRange(wTarget))
            {
                Variable.W.Cast(Variable.Player, Variable.Config.Item("UsePackets").GetValue<bool>());
            }

            if (Variable.Config.Item("UseEH").GetValue<bool>() && Variable.E.IsReady() && eTarget != null &&
                Variable.Player.Distance(eTarget) <= Variable.E.Range)
            {
                Variable.E.Cast();
            }

            if (!Variable.Config.Item("HMinions").GetValue<bool>() || Variable.Config.Item("SupportMode").GetValue<bool>())
            {
                return;
            }

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>()
                .Where(
                    minion =>
                        minion.Distance(eTarget) <= 150 && eTarget != null))
            {
                Variable.Orbwalker.ForceTarget(minion);
            }
        }

        public static void Farm(bool laneClear)
        {
            var allMinionsQ = MinionManager.GetMinions(Variable.Player.ServerPosition, Variable.Q.Range);
            var allMinionsE = MinionManager.GetMinions(Variable.Player.ServerPosition, Variable.E.Range + 150);

            var useQi = Variable.Config.Item("UseQF").GetValue<StringList>().SelectedIndex;
            var useEi = Variable.Config.Item("UseEF").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && Variable.Q.IsReady())
            {
                foreach (var minion in allMinionsQ.Where(minion => !Orbwalking.InAutoAttackRange(minion) &&
                                                                   minion.Health <
                                                                   Variable.Player.GetSpellDamage(minion, SpellSlot.Q)))
                {
                    Variable.Q.Cast(minion, Variable.Config.Item("UsePackets").GetValue<bool>());
                }
            }
            if (!useE || !Variable.E.IsReady())
            {
                return;
            }

            if (laneClear)
            {
                foreach (var minion in allMinionsE.Where(minion => Variable.Player.Distance(minion) <= Variable.E.Range)
                    )
                {
                    Variable.E.Cast();
                }

                foreach (var minion in allMinionsE
                    .Where(
                        eMinion =>
                            Variable.Player.Distance(eMinion) > Variable.E.Range &&
                            Variable.Player.Distance(eMinion) <= Variable.E.Range + 150)
                    .SelectMany(eMinion => ObjectManager.Get<Obj_AI_Minion>()
                        .Where(minion => eMinion.Distance(minion) <= 150 && eMinion != minion)))
                {
                    Variable.Orbwalker.ForceTarget(minion);
                }
            }
            else
            {
                foreach (var minion in allMinionsE
                    .Where(
                        minion =>
                            !Orbwalking.InAutoAttackRange(minion) &&
                            !Variable.RighteousFuryActive))
                {
                    Variable.E.Cast();
                }
            }
        }

        public static void JungleFarm()
        {
            var useQ = Variable.Config.Item("UseQJ").GetValue<bool>();
            var useE = Variable.Config.Item("UseEJ").GetValue<bool>();

            var mobs = MinionManager.GetMinions(Variable.Player.ServerPosition, Variable.W.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count <= 0)
            {
                return;
            }

            var mob = mobs[0];
            if (useQ && Variable.Q.IsReady())
            {
                Variable.Q.Cast(mob, Variable.Config.Item("UsePackets").GetValue<bool>());
            }

            if (useE && Variable.E.IsReady())
            {
                Variable.E.Cast();
            }
        }

        public static void Ultimate()
        {
            foreach (var ally in from ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(ally => ally.IsAlly && !ally.IsDead && Variable.Player.CountEnemysInRange(1000) > 0)
                let menuItem = Variable.Config.Item("Ult" + ally.ChampionName).GetValue<bool>()
                where
                    menuItem &&
                    Variable.Config.Item("UltMinHP").GetValue<Slider>().Value >= (ally.Health/ally.MaxHealth)*100 &&
                    Variable.R.IsReady() && !Variable.Player.IsRecalling()
                select ally)
            {
                Variable.R.Cast(ally, Variable.Config.Item("UsePackets").GetValue<bool>());
            }
        }

        public static void Heal()
        {
            foreach (var ally in from ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(ally => ally.IsAlly && !ally.IsDead)
                let menuItem = Variable.Config.Item("Heal" + ally.ChampionName).GetValue<bool>()
                where
                    menuItem &&
                    Variable.Config.Item("HealMinHP").GetValue<Slider>().Value >= (ally.Health/ally.MaxHealth)*100 &&
                    Variable.W.IsReady() && !Variable.Player.IsRecalling()
                select ally)
            {
                Variable.W.Cast(ally, Variable.Config.Item("UsePackets").GetValue<bool>());
            }
        }

        public static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (Variable.Q.IsReady())
            {
                damage += Variable.Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (Variable.Dfg.IsReady())
            {
                damage += Variable.Player.GetItemDamage(enemy, Damage.DamageItems.Dfg)/1.2;
            }

            if (Variable.E.IsReady())
            {
                damage += Variable.Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (Variable.IgniteSlot != SpellSlot.Unknown &&
                Variable.Player.Spellbook.CanUseSpell(Variable.IgniteSlot) == SpellState.Ready)
            {
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage*(Variable.Dfg.IsReady() ? 1.2f : 1);
        }

        public static void DrawCircles()
        {
            foreach (var spell in Variable.SpellList)
            {
                var menuItem = Variable.Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(Variable.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }
    }
}