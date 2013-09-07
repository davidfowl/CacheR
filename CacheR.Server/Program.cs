using System;
using System.Diagnostics;
using System.Linq;

namespace CacheR.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new ConsoleTraceListener();
            string url = args.Length == 1 ? args[0] : "http://localhost:8087/";

            var server = new CacheServer(url);
            server.Start();

            Console.WriteLine("Running cache server on {0}", url);
            Console.WriteLine("Press 'q' to quit.");
            Console.WriteLine("Press 'v' to view the cache data.");
            Console.WriteLine("Press 'd' to enable debug mode.");

            var uri = new Uri(url);
            string prompt = String.Format("[{0}:{1}]: ", uri.Host, uri.Port);

            while (true)
            {
                Console.Write(prompt);
                var ki = Console.ReadKey();
                Console.WriteLine();
                if (ki.Key == ConsoleKey.Q)
                {
                    break;
                }

                if (ki.Key == ConsoleKey.D)
                {
                    if (Debug.AutoFlush)
                    {
                        Debug.Listeners.Remove(listener);
                    }
                    else
                    {
                        Debug.Listeners.Add(listener);
                    }

                    Debug.AutoFlush = !Debug.AutoFlush;
                    Log("Turning debugging {0}.", Debug.AutoFlush ? "on" : "off");
                }

                if (ki.Key == ConsoleKey.V)
                {
                    var entries = server.Store.GetAll().Take(100).ToList();
                    if (entries.Count == 0)
                    {
                        Log("Nothing in the cache.");
                    }
                    else
                    {
                        foreach (var item in entries)
                        {
                            Console.WriteLine(item.Key + " = " + item.Value);
                        }
                    }
                }
            }
        }

        private static void Log(string value, params object[] args)
        {
            Console.WriteLine("[" + DateTime.Now + "]: " + value, args);
        }

        private static void Log(string value)
        {
            Console.WriteLine("[" + DateTime.Now + "]: " + value);
        }
    }
}
