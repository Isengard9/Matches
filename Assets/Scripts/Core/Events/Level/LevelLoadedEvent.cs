namespace Core.Events.Level
{
    public struct LevelLoadedEvent
    {
        public ILevel Level { set; get; }
    }
}