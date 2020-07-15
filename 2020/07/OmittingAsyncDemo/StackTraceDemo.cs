using System;
using System.Threading.Tasks;

namespace OmittingAsyncDemo
{
    static class StackTraceDemo
    {
        public static void Run()
        {
            Console.WriteLine("=== Running With Keywords ===");
            try
            {
                WithKeywords.Top().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("\n=== Running Without Keywords ===");
            try
            {
                WithoutKeywords.Top().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static class WithKeywords
        {
            public static async Task Top()
            {
                await Middle();
            }

            static async Task Middle()
            {
                await Bottom();
            }

            static async Task Bottom()
            {
                await Task.Delay(10);
                throw new Exception("Bottom Exception");
            }
        }

        static class WithoutKeywords
        {
            public static async Task Top()
            {
                await Middle();
            }

            static Task Middle()
            {
                return Bottom();
            }

            static async Task Bottom()
            {
                await Task.Delay(10);
                throw new Exception("Bottom Exception");
            }
        }
    }

}
