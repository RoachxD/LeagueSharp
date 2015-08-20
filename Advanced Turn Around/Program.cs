using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Advanced_Turn_Around
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += delegate
            {
                var onGameLoadThread = new Thread(Game_OnGameLoad);
                onGameLoadThread.Start();
            };
        }

        private static void Game_OnGameLoad()
        {
            Internal.AddChampions();

            Variable.Config = new Menu("Roach's Turn Around#", "ATA", true);

            Variable.Config.AddItem(new MenuItem("Enabled", "Enable the Script").SetValue(true));

            Variable.Config.AddSubMenu(new Menu("Champions and Spells", "CAS"));
            foreach (var champ in Variable.ExistingChampions)
            {
                Variable.Config.SubMenu("CAS")
                    .AddSubMenu(new Menu(champ.CharName + "'s Spells to Avoid", champ.CharName));
                Variable.Config.SubMenu("CAS")
                    .SubMenu(champ.CharName)
                    .AddItem(new MenuItem(champ.Key, champ.SpellName).SetValue(true));
            }

            Variable.Config.AddToMainMenu();

            Game.PrintChat(
                "<font color=\"#FF440A\">Advanced Turn Around# -</font> <font color=\"#FFFFFF\">Loaded</font>");

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Variable.Config.Item("Enabled").GetValue<bool>() || !Variable.Player.IsTargetable ||
                (sender == null || sender.Team == Variable.Player.Team))
            {
                return;
            }

            foreach (var champ in Variable.ExistingChampions)
            {
                if ((Variable.Config.SubMenu("CAS").SubMenu(champ.CharName) == null) ||
                    (Variable.Config.SubMenu("CAS").SubMenu(champ.CharName).Item(champ.Key) == null) ||
                    (!Variable.Config.SubMenu("CAS").SubMenu(champ.CharName).Item(champ.Key).GetValue<bool>()))
                {
                    continue;
                }

                if (!args.SData.Name.Contains(champ.Key) ||
                    (!(Variable.Player.Distance(sender.Position) <= champ.Range) && args.Target != Variable.Player))
                {
                    continue;
                }

                var vector =
                    new Vector3(
                        Variable.Player.Position.X +
                        ((sender.Position.X - Variable.Player.Position.X)*(Internal.MoveTo(champ.Movement))/
                         Variable.Player.Distance(sender.Position)),
                        Variable.Player.Position.Y +
                        ((sender.Position.Y - Variable.Player.Position.Y)*(Internal.MoveTo(champ.Movement))/
                         Variable.Player.Distance(sender.Position)), 0);
                Variable.Player.IssueOrder(GameObjectOrder.MoveTo, vector);
                Orbwalking.Move = false;
                Utility.DelayAction.Add((int) (champ.CastTime + 0.1)*1000, () => Orbwalking.Move = true);
            }
        }
    }
}
