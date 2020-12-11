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

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\sofachair_silo.swf", false, 0,2, 2, RenderShadows: false);
            furni.Run();

            furni.BuildImage();
            //File.WriteAllBytes(furni.GetFileName(), furni.CreateImage());

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\sofachair_silo.swf", false, 0, 4, 2, RenderShadows: false);
            furni.Run();

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\sofachair_silo.swf", false, 0, 6, 2, RenderShadows: false);
            furni.Run();

            Console.WriteLine("Finished conversion");
            Console.Read();
        }
    }
}
