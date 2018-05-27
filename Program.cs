using System;

namespace KiraNet.GutsMvc.BBS
{
    class Program
    {
        static void Main(string[] args)
        {
            new WebHostBuilder()
               .InitialRoot(@"D:\Code\KiraNet.GutsMvc.BBS\KiraNet.GutsMvc.BBS")
               .UseHttpListener()
               .UseUrls("http://+:17758/")
               .UseStartup<Startup>()
               .Build()
               .Start();

            Console.ReadKey();
        }
    }
}
