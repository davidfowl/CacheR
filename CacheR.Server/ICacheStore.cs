using System.Collections.Generic;
using System.Threading.Tasks;
using CacheR.Model;

namespace CacheR.Server
{
    public interface ICacheStore
    {
        Task Save(CacheEntry entry);
        IEnumerable<CacheEntry> GetAll();
        Task Delete(string key);
    }
}
