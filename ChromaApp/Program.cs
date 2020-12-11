using Chroma;
using System;

namespace ChromaApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ChromaFurniture furni;

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\ads_711c.swf", false, 0, 2, 2);
            furni.Run();


            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\ads_711c.swf", false, 0, 4, 2);
            furni.Run();


            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\sofachair_silo.swf", false, 0, 4, 2, RenderShadows: false);
            furni.Run();

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\pura_mdl2.swf", false, 0, 2, 1);
            furni.Run();

            Console.WriteLine("Finished conversion");
            Console.Read();
        }
    }
}
