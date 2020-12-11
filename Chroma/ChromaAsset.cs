using System;
using System.Drawing;
using System.IO;
using System.Xml;

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
                Direction = int.Parse(data[2]);
                Frame = int.Parse(data[3]);

                var xmlData = FileUtil.SolveXmlFile(chromaFurniture.XmlDirectory, "visualization");

                XmlNodeList layers = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (chromaFurniture.IsSmallFurni ? "32" : "64") + "']/layers/layer");

                if (layers == null || layers.Count == 0)
                {
                    layers = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (chromaFurniture.IsSmallFurni ? "32" : "64") + "']/directions/direction[@id='" + chromaFurniture.RenderDirection + "']/layer");
                }

                for (int i = 0; i < layers.Count; i++)
                {
                    var layer = layers.Item(i);
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
            }
            catch (FormatException)
            {
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

                if (!File.Exists(newPath))
                    File.Copy(file, newPath);

                if (flipH)
                {
                    var bitmap1 = (Bitmap)Bitmap.FromFile(newPath);

                    RelativeX = bitmap1.Width - RelativeX;
                    ImageX = RelativeX;

                    bitmap1.RotateFlip(RotateFlipType.Rotate180FlipY);
                    bitmap1.Save(newPath);
                    bitmap1.Dispose();
                }
            }
        }

        public string GetImagePath()
        {
            return FileUtil.SolveFile(chromaFurniture.OutputDirectory, imageName);
        }
    }
}