using System.Collections.Generic;
using LeagueSharp;

namespace Stun_Alerter
{
    internal class Stunner
    {
        public static void InitializeSpells()
        {
            RegisterSpell("Aatrox", new List<SpellSlot> {SpellSlot.Q});
            RegisterSpell("Ahri", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Alistar", new List<SpellSlot> {SpellSlot.Q, SpellSlot.W});
            RegisterSpell("Amumu", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Anivia", new List<SpellSlot> {SpellSlot.Q});
            RegisterSpell("Ashe", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Azir", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Bard", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Blitzcrank", new List<SpellSlot> {SpellSlot.Q, SpellSlot.E, SpellSlot.R});
            RegisterSpell("Braum", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Cassiopeia", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Chogath", new List<SpellSlot> {SpellSlot.Q, SpellSlot.E});
            RegisterSpell("Darius", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Diana", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Draven", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Ekko", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("Fiddlesticks", new List<SpellSlot> {SpellSlot.Q, SpellSlot.E});
            RegisterSpell("Fizz", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Galio", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Garen", new List<SpellSlot> {SpellSlot.Q});
            RegisterSpell("Gnar", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Gragas", new List<SpellSlot> {SpellSlot.E, SpellSlot.R});
            RegisterSpell("Hecarim", new List<SpellSlot> {SpellSlot.E, SpellSlot.R});
            RegisterSpell("Heimerdinger", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Irelia", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Janna", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Jax", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Karma", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("Kassadin", new List<SpellSlot> {SpellSlot.Q});
            RegisterSpell("LeeSin", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Leona", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Lissandra", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Lulu", new List<SpellSlot> {SpellSlot.W, SpellSlot.R});
            RegisterSpell("Malphite", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Malzahar", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Maokai", new List<SpellSlot> {SpellSlot.Q});
            RegisterSpell("Morgana", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Nami", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Nautilus", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Nocturne", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Orianna", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Pantheon", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("Poppy", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Quinn", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Rammus", new List<SpellSlot> {SpellSlot.Q, SpellSlot.E});
            RegisterSpell("RekSai", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("Renekton", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("Riven", new List<SpellSlot> {SpellSlot.Q, SpellSlot.W});
            RegisterSpell("Sejuani", new List<SpellSlot> {SpellSlot.E, SpellSlot.R});
            RegisterSpell("Shen", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Shivana", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Singed", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Sion", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Skarner", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Sona", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Soraka", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Syndra", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Taric", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Thresh", new List<SpellSlot> {SpellSlot.Q, SpellSlot.E});
            RegisterSpell("Tristana", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Trundle", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("TwistedFate", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("Udyr", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Urgot", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Vayne", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Veigar", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("VelKoz", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("Vi", new List<SpellSlot> {SpellSlot.Q, SpellSlot.R});
            RegisterSpell("Viktor", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("Volibear", new List<SpellSlot> {SpellSlot.Q});
            RegisterSpell("Warwick", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Wukong", new List<SpellSlot> {SpellSlot.R});
            RegisterSpell("Xerath", new List<SpellSlot> {SpellSlot.E});
            RegisterSpell("XinZhao", new List<SpellSlot> {SpellSlot.Q});
            RegisterSpell("Zac", new List<SpellSlot> {SpellSlot.E, SpellSlot.R});
            RegisterSpell("Ziggs", new List<SpellSlot> {SpellSlot.W});
            RegisterSpell("Zyra", new List<SpellSlot> {SpellSlot.R});
        }

        private static void RegisterSpell(string champName, List<SpellSlot> spellSlots)
        {
            if (!Variable.StunSpells.ContainsKey(champName))
            {
                Variable.StunSpells.Add(champName, spellSlots);
            }
        }
    }
}