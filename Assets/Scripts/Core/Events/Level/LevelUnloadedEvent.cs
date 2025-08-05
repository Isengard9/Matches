namespace Core.Events.Level
{
    public struct LevelUnloadedEvent
    {
        public ILevel Level { get; set; }
    }
}