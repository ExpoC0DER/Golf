namespace _game.Scripts
{
    public static class Enums
    {
        public enum Layer
        {
            Player = 6,
            Ghost = 7,
            Obstacle = 8,
            ObstaclePreview = 9,
            IgnorePreviewRender = 10
        }

        public enum GamePhase
        {
            Menu,
            Play,
            Build,
            ObstacleSelection,
            RoundEnd,
            GameEnd
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
