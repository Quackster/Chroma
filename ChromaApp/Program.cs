using Chroma;
using Extractor;
using System;

namespace ChromaApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\ads_clwall2.swf", false, 0, 0, 0);
            furni.Run();

            Console.WriteLine("Finished conversion");
            Console.Read();
        }
    }
}
