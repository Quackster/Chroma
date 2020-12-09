using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chroma
{
    public class FileUtil
    {
        public static XmlDocument SolveXmlFile(string outputDirectory, string fileNameContains)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(outputDirectory, "binaryData"), "*"))
            {
                if (Path.GetFileNameWithoutExtension(file).Contains(fileNameContains))
                {
                    var text = File.ReadAllText(file);

                    if (text.Contains("\n<?xml"))
                    {
                        text = text.Replace("\n<?xml", "<?xml");
                        File.WriteAllText(file, text);
                    }

                    if (text.Contains("<graphics>"))
                    {
                        text = text.Replace("<graphics>", "");
                        text = text.Replace("</graphics>", "            ");
                        File.WriteAllText(file, text);
                    }

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);

                    return xmlDoc;
                }
            }

            return null;
        }

        public static string SolveFile(string outputDirectory, string fileNameContains, bool endsWith = true)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(outputDirectory, "images"), "*"))
            {
                if (endsWith)
                {
                    if (Path.GetFileNameWithoutExtension(file).EndsWith(fileNameContains))
                    {
                        return file;
                    }
                }
                else
                {
                    if (Path.GetFileNameWithoutExtension(file).Contains(fileNameContains))
                    {
                        return file;
                    }
                }
            }

            return null;
        }

        public static string NumericLetter(int animationLayer)
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            return Convert.ToString(alphabet[animationLayer]).ToLower();
        }
    }
}
