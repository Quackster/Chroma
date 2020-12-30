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

            furni = new ChromaFurniture(@"C:\Users\Alex\Documents\Habbo Furni\gamedata-2020-14-12\swf_furni\hc_tbl.swf", isSmallFurni: false, renderState: 1, renderDirection: 0, colourId: 2);
            furni.Run();

            File.WriteAllBytes(furni.GetFileName(), furni.CreateImage());

            Console.WriteLine("Finished conversion");
            Console.Read();
        }
    }
}
