using Engine.Models;

namespace Engine.ViewModels
{
    public class GameSession
    {
        public Player CurrentPlayer { get; set; }

        public GameSession ()
        {
            //CurrentPlayer = new Player(10, 10, 20, 0, 1);
            CurrentPlayer = new Player(10, 10, 100000, 0, 1, "Assassin");
            CurrentPlayer.Name = "Alexander";
            CurrentPlayer.CharacterClass = "Assassin";
            CurrentPlayer.HitPoints = 10;
            CurrentPlayer.Gold = 100000;
            CurrentPlayer.ExperiencePoints = 0;
            CurrentPlayer.Level = 1;
        }
    }
}
