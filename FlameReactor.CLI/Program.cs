using System;

namespace FlameReactor.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var emberService = new EmberService("./");
            while(true)
            {
                var x = emberService.Breed();
                x.Wait();
            }
            Console.WriteLine("Auth?");
        }
    }
}
