using System;
using LeagueSharp;

namespace Advanced_Turn_Around
{
    internal class Internal
    {
        public static void AddChampions()
        {
            Variable.ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "Cassiopeia",
                    Slot = SpellSlot.R,
                    Range = 1000,
                    SpellName = "Petrifying Gaze (R)",
                    Movement = Variable.MovementDirection.Backward,
                    CastTime = 1.5f
                });

            Variable.ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "Tryndamere",
                    Slot = SpellSlot.W,
                    Range = 900,
                    SpellName = "Mocking Shout (W)",
                    Movement = Variable.MovementDirection.Forward,
                    CastTime = 0.65f
                });
        }

        public static int MoveTo(Variable.MovementDirection direction)
        {
            switch (direction)
            {
                case Variable.MovementDirection.Forward:
                    return 100;
                case Variable.MovementDirection.Backward:
                    return -100;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }

        public class ChampionInfo
        {
            public string CharName { get; set; }
            public SpellSlot Slot { get; set; }
            public float Range { get; set; }
            public string SpellName { get; set; }
            public Variable.MovementDirection Movement { get; set; }
            public float CastTime { get; set; }
        }
    }
}