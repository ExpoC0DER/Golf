namespace _game.Scripts
{
    public abstract class Enums
    {
        public enum Layer
        {
            Player = 6,
            Ghost = 7,
            Obstacle = 8,
            ObstaclePreview = 9
        }

        public enum GamePhase
        {
            Play,
            Build,
            Intermission
        }

        public enum ActionMap
        {
            Player,
            Build,
            Menu
        }
        
        public enum Tags
        {
            Hole,
            Obstacle,
            Wall
        }
        
        public enum Iterate
        {
            Next,
            Previous
        }
    }
}
