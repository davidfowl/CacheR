using System;
using System.Threading.Tasks;
using CacheR.Client;

namespace CacheR
{
    class Program
    {
        static void Main(string[] args)
        {
            var cache = new Cache("http://localhost:8087");

            RunCacheTest(cache).Wait();
        }

        private static async Task RunCacheTest(Cache cache)
        {
            await cache.ConnectAsync();

            Console.WriteLine("Enter 'key=value' to add a value to the cache.");
            Console.WriteLine("Enter 'key' to get a value from the cache.");
            Console.WriteLine("Enter '-key' to delete a value from the cache.");

            string line = null;
            while ((line = Console.ReadLine()) != null)
            {
                var values = line.Split('=');
                if (values.Length == 2)
                {
                    string key = values[0].Trim();
                    string value = values[1].Trim();
                    await cache.AddAsync(key, value);

                    Console.WriteLine("Added '{0}' to the cache with value '{1}'.", key, value);
                }
                else if (line.StartsWith("-"))
                {
                    string key = line.Substring(1).Trim();
                    await cache.DeleteAsync(key);

                    Console.WriteLine("Deleting entry for key '{0}'", key);
                }
                else
                {
                    string key = line.Trim();
                    Console.WriteLine("Value for '{0}' is " + cache.Get(key), key);
                }
            }
        }
    }
}
