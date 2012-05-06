using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CacheR.Model;
using Newtonsoft.Json;

namespace CacheR.Server
{
    public class FileCacheStore : ICacheStore
    {
        private readonly string _path;

        public FileCacheStore(string path)
        {
            _path = path;
        }

        public Task Save(CacheEntry entry)
        {
            string rawEntry = JsonConvert.SerializeObject(entry);
            return Task.Factory.StartNew(() => File.AppendAllText(_path, rawEntry + Environment.NewLine));
        }

        public IEnumerable<CacheEntry> GetAll()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    return Enumerable.Empty<CacheEntry>();
                }

                // This is inefficient but meh :)
                return (from line in File.ReadAllLines(_path)
                        select line.Trim() into entry
                        select JsonConvert.DeserializeObject<CacheEntry>(entry))
                        .Reverse()
                        .Distinct(CacheEntryKeyComparer.Instance);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to retrive cached entries: " + ex);
                return Enumerable.Empty<CacheEntry>();
            }
        }
    }
}
