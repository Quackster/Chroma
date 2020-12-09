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
                ProcessSWF(@"chair_silo.swf");
                ProcessSWF(@"tv_flat.swf");
                ProcessSWF("computer_old.swf");
                //ProcessSWF("kinkysofa.swf");
                //ProcessSWF(@"C:\Users\Alex\Documents\gamedata-2020-18-2\hof_furni\christmas_sleigh.swf");
                ProcessSWF("waasa_bunk_bed.swf");
                //ProcessSWF(@"school_chairgold.swf");
                //ProcessSWF(@"C:\Users\Alex\Documents\gamedata-2020-18-2\hof_furni\bardesk_polyfon.swf");
                //ProcessSWF(@"C:\Users\Alex\Documents\gamedata-2020-18-2\hof_furni\lamp_armas.swf");
                ProcessSWF("computer_laptop.swf");
                ProcessSWF("rare_beehive_bulb.swf");

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
