using System.Drawing;
using LeagueSharp;

namespace Stun_Alerter
{
    internal static class Internal
    {
        public static SharpDX.ColorBGRA SharpDXConverter(Color c)
        {
            return new SharpDX.ColorBGRA(c.R, c.G, c.B, c.A);
        }

        public static bool CanStun(this Obj_AI_Hero hero, SpellSlot slot)
        {
            return hero.Spellbook.CanUseSpell(slot) == SpellState.Ready ||
                   hero.Spellbook.CanUseSpell(slot) == SpellState.Surpressed;
        }
    }
}