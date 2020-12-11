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

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\tv_flat.swf", false, 1, 0, 2, RenderShadows: false);
            furni.Run();

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\tv_flat.swf", false, 1, 2, 2, RenderShadows: false);
            furni.Run();

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\tv_flat.swf", false, 1, 4, 2, RenderShadows: false);
            furni.Run();

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\tv_flat.swf", false, 1, 6, 2, RenderShadows: false);
            furni.Run();


            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\wooden_screen.swf", false, 1, 0, 1, RenderShadows: false);
            furni.Run();

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\wooden_screen.swf", false, 1, 2, 2, RenderShadows: false);
            furni.Run();

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\wooden_screen.swf", false, 1, 4, 3, RenderShadows: false);
            furni.Run();

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\wooden_screen.swf", false, 1, 6, 4, RenderShadows: false);
            furni.Run();

            Console.WriteLine("Finished conversion");
            Console.Read();
        }
    }
}
