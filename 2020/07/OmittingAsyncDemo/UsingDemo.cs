using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OmittingAsyncDemo
{
    static class UsingDemo
    {
        public static void Run()
        {
            try
            {
                UsingStatementWithoutKeywords().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static Task<string> UsingStatementWithoutKeywords()
        {
            using var client = new HttpClient();
            return client.GetStringAsync("https://1.1.1.1");
        }
    }
}
