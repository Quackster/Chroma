using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chroma
{
    class Program
    {
        static void Main(string[] args)
        {
            //try
            {
                //ProcessSWF("throne.swf");
                ProcessSWF("prizetrophy6.swf");

                //ProcessSWF(@"C:\Users\Alex\Documents\gamedata-2020-18-2\hof_furni\chair_silo.swf");
            }
            //catch (Exception ex)
            {
                //string exception = ex.ToString();
                //Console.WriteLine(exception);
            }

            Console.WriteLine("Done!");
            //Console.ReadLine();
        }

        private static void ProcessSWF(string fileName)
        {
            if (!File.Exists(fileName))
            {
                string webName = Path.GetFileName(fileName);
                var webClient = new WebClient();
                webClient.DownloadFile("http://cdn.classichabbo.com/r38/dcr/hof_furni/" + webName, webName);
            }
            ChromaFurniture furniture = null;

            furniture = new ChromaFurniture(fileName, "furniture_export", false, renderState: 0, renderDirection: 2, colourId: 3);
            furniture.Run();

            furniture = new ChromaFurniture(fileName, "furniture_export", false, renderState: 1, renderDirection: 2, colourId: 1);
            furniture.Run();
        }

    }
}
