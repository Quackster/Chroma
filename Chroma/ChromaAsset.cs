using System;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Linq;
using Newtonsoft.Json;

namespace Chroma
{
    public class ChromaAsset
    {
        public int RelativeX;
        public int RelativeY;
        public int ImageX;
        public int ImageY;
        public int Z = -1;
        public string sourceImage;
        public string imageName;
        public bool flipH;
        public int Layer;
        public int Direction;
        public int Frame;
        public bool IsSmall;
        private ChromaFurniture chromaFurniture;
        public string Ink;
        public bool IgnoreMouse;
        public bool Shadow;
        public string ColourCode;
        public int Alpha = -1;
        public ChromaAsset(ChromaFurniture chromaFurniture, int x, int y, string sourceImage, string imageName)
        {
            this.chromaFurniture = chromaFurniture;
            this.RelativeX = x;
            this.RelativeY = y;
            this.ImageX = x;
            this.ImageY = y;
            this.sourceImage = sourceImage;
            this.imageName = imageName;
           
        }

        public bool Parse()
        {
            try
            {
                string dataName = imageName.Replace(chromaFurniture.Sprite + "_", "");
                string[] data = dataName.Split('_');

                IsSmall = (data[0] == "32");
                Layer = (data[1].ToUpper().ToCharArray()[0] - 64) - 1;
                Direction = chromaFurniture.IsIcon ? 0 : int.Parse(data[2]);
                Frame = chromaFurniture.IsIcon ? 0 : int.Parse(data[3]);

                var xmlData = FileUtil.SolveXmlFile(chromaFurniture.XmlDirectory, "visualization");

                XmlNodeList visualisationLayers = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (chromaFurniture.IsSmallFurni ? "32" : "64") + "']/layers/layer");

                if (visualisationLayers == null || visualisationLayers.Count == 0)
                {
                    visualisationLayers = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (chromaFurniture.IsSmallFurni ? "32" : "64") + "']/directions/direction[@id='" + chromaFurniture.RenderDirection + "']/layer");
                }

                for (int i = 0; i < visualisationLayers.Count; i++)
                {
                    var layer = visualisationLayers.Item(i);
                    var animationLayer = int.Parse(layer.Attributes.GetNamedItem("id").InnerText);

                    if (animationLayer == this.Layer)
                    {

                        if (layer.Attributes.GetNamedItem("ink") != null)
                            Ink = layer.Attributes.GetNamedItem("ink").InnerText;

                        if (layer.Attributes.GetNamedItem("ignoreMouse") != null)
                            IgnoreMouse = layer.Attributes.GetNamedItem("ignoreMouse").InnerText == "1";

                        //if (layer.Attributes.GetNamedItem("ink") != null)
                        //    InkAdd = layer.Attributes.GetNamedItem("ink").InnerText == "ADD";

                        if (layer.Attributes.GetNamedItem("z") != null)
                            Z = int.Parse(layer.Attributes.GetNamedItem("z").InnerText);

                        if (layer.Attributes.GetNamedItem("alpha") != null)
                            Alpha = int.Parse(layer.Attributes.GetNamedItem("alpha").InnerText);
                    }
                }

                if (Z == -1)
                {
                    Z = Layer;
                }

                if (chromaFurniture.ColourId > -1)
                {
                    XmlNodeList colorLayers = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (chromaFurniture.IsSmallFurni ? "32" : "64") + "']/colors/color[@id='" + chromaFurniture.ColourId + "']/colorLayer[@id='" + Layer + "']");

                    if (colorLayers.Count > 0)
                    {
                        ColourCode = colorLayers.Item(0).Attributes.GetNamedItem("color").InnerText;
                    }
                }

                /*if (chromaFurniture.Animations.ContainsKey(Layer) && 
                    chromaFurniture.Animations[Layer].States.Count > 0 && 
                    chromaFurniture.Animations[Layer].States.ContainsKey(chromaFurniture.RenderState)) 
                {
                    var json = JsonConvert.SerializeObject(chromaFurniture.Animations[Layer].States[chromaFurniture.RenderState]);
                    Frame = int.Parse(chromaFurniture.Animations[Layer].States[chromaFurniture.RenderState].Frames[0]);
                }*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public void GenerateImage()
        {
            var file = FileUtil.SolveFile(chromaFurniture.OutputDirectory, sourceImage);

            if (file != null)
            {
                var newName = imageName + ".png";
                var newPath = Path.Combine(chromaFurniture.OutputDirectory, newName);

                if (File.Exists(newPath))
                {
                    if (flipH)
                    {
                        var bitmap1 = (Bitmap)Bitmap.FromFile(newPath);

                        RelativeX = bitmap1.Width - RelativeX;
                        ImageX = RelativeX;

                        bitmap1.Dispose();
                    }
                }
            }
        }

        public string GetImagePath()
        {
            return FileUtil.SolveFile(chromaFurniture.OutputDirectory, imageName);
        }
    }
}