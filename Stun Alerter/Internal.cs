using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Stun_Alerter
{
    internal static class Internal
    {
        public static ColorBGRA SharpDXConverter(Color c)
        {
            return new ColorBGRA(c.R, c.G, c.B, c.A);
        }

        public static void Ping(Vector2 position)
        {
            if (Utils.TickCount - Variable.LastPing < (30 * 1000))
            {
                return;
            }

            Variable.LastPing = Utils.TickCount;

            Utility.DelayAction.Add(150, () => Game.ShowPing(PingCategory.Fallback, position, true));
            Utility.DelayAction.Add(300, () => Game.ShowPing(PingCategory.Fallback, position, true));
            Utility.DelayAction.Add(400, () => Game.ShowPing(PingCategory.Fallback, position, true));
            Utility.DelayAction.Add(800, () => Game.ShowPing(PingCategory.Fallback, position, true));
        }

        public static bool CanStun(this Obj_AI_Hero hero, SpellSlot slot)
        {
            return hero.Spellbook.CanUseSpell(slot) == SpellState.Ready ||
                   hero.Spellbook.CanUseSpell(slot) == SpellState.Surpressed;
        }
    }
}