using System.Collections.Generic;

namespace CacheR.Model
{
    public class CacheEntryKeyComparer : IEqualityComparer<CacheEntry>
    {
        public readonly static CacheEntryKeyComparer Instance = new CacheEntryKeyComparer();

        private CacheEntryKeyComparer()
        {
        }

        public bool Equals(CacheEntry x, CacheEntry y)
        {
            return x.Key.Equals(y.Key);
        }

        public int GetHashCode(CacheEntry obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}
