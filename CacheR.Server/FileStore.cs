using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CacheR.Model;
using Newtonsoft.Json;

namespace CacheR.Server
{
    public class FileStore : ICacheStore
    {
        private static IQueryable<CacheEntry> _empty = Enumerable.Empty<CacheEntry>().AsQueryable();

        private readonly string _path;

        public FileStore(string path)
        {
            _path = path;
        }

        public Task Save(CacheEntry entry)
        {
            string rawEntry = JsonConvert.SerializeObject(entry);
            return Task.Factory.StartNew(() => File.AppendAllText(_path, rawEntry + Environment.NewLine));
        }

        public IQueryable<CacheEntry> GetAll()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    return _empty;
                }

                // This is inefficient but meh :)
                return (from line in File.ReadAllLines(_path)
                        select line.Trim() into entry
                        select JsonConvert.DeserializeObject<CacheEntry>(entry))
                        .Reverse()
                        .Distinct(CacheEntryKeyComparer.Instance)
                        .AsQueryable();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to retrive cached entries: " + ex);
                return _empty;
            }
        }
    }
}
