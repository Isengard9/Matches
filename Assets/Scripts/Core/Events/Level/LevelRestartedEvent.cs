namespace Core.Events.Level
{
    public struct LevelRestartedEvent
    {
        public ILevel Level { get; set; }
    }
}