using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OmittingAsyncDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Running Stack Trace Demo ===\n");
            StackTraceDemo.Run();

            Console.WriteLine("=== Running Stack Trace Demo ===\n");
            UsingDemo.Run();
        }

    }

    static class UsingDemo
    {
        public static void Run()
        {
            Console.WriteLine(UsingStatementWithoutKeywords().GetAwaiter().GetResult());
        }

        static Task<string> UsingStatementWithoutKeywords()
        {
            using var client = new HttpClient();
            return client.GetStringAsync("https://1.1.1.1");
        }
    }
}
