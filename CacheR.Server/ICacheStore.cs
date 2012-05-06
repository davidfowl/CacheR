using System.Linq;
using System.Threading.Tasks;
using CacheR.Model;

namespace CacheR.Server
{
    public interface ICacheStore
    {
        Task Save(CacheEntry entry);
        IQueryable<CacheEntry> GetAll();
    }
}
