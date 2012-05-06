namespace CacheR.Model
{
    public class CacheCommand
    {
        public CacheCommandType Type { get; set; }
        public CacheEntry[] Entries { get; set; }
    }
}
