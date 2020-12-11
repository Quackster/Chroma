using Chroma;
using System;
using System.IO;

namespace ChromaApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ChromaFurniture furni;

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\tv_flat.swf", isSmallFurni: false, renderState: 0, renderDirection: 2);
            furni.Run();

            File.WriteAllBytes(furni.GetFileName(), furni.CreateImage());

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\chair_silo.swf", isSmallFurni: false, renderState: 0, renderDirection: 2, colourId: 2);
            furni.Run();

            File.WriteAllBytes(furni.GetFileName(), furni.CreateImage());

            Console.WriteLine("Finished conversion");
            Console.Read();
        }
    }
}
