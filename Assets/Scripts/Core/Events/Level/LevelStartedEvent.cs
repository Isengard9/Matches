namespace Core.Events.Level
{
    public struct LevelStartedEvent
    {
        public ILevel Level { set; get; }
    }
}