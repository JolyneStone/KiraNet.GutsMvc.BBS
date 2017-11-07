using System;

namespace KiraNet.GutsMvc.BBS
{
    class Program
    {
        static void Main(string[] args)
        {
            new WebHostBuilder()
               .UseHttpListener()
               .UseUrls("http://+:17758/")
               .UseStartup<Startup>()
               .Build()
               .Start();

            Console.ReadKey();
        }
    }
}
