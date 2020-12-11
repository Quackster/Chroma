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

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\GitHub\Havana\tools\www\r38\dcr\hof_furni\tv_flat.swf", false, 0, 2, 2);

            string fileName = furni.Run();
            byte[] buffer = furni.CreateImage();

            File.WriteAllBytes(fileName, buffer);

            Console.WriteLine("Finished conversion");
            Console.Read();
        }
    }
}
