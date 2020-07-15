using System;

namespace OmittingAsyncDemo
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Running Stack Trace Demo ===\n");
            StackTraceDemo.Run();

            Console.WriteLine("\n\n=== Running Stack Trace Demo ===\n");
            UsingDemo.Run();
        }
    }
}