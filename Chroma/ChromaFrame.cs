using System.Drawing;

namespace Chroma
{
    public class ChromaFrame
    {
        private ChromaFurniture chromaFurniture;
        public int AnimationLayer;
        public int RenderFrame;
        public string Source;
        public string Name;
        public int X;
        public int Y;
        public int Z;

        public ChromaFrame(ChromaFurniture chromaFurniture, int animationLayer, int renderFrame)
        {
            this.chromaFurniture = chromaFurniture;
            this.AnimationLayer = animationLayer;
            this.RenderFrame = renderFrame;
        }

        public string BuildAssetName()
        {
            string letter = FileUtil.NumericLetter(AnimationLayer);
            var builtString = chromaFurniture.Sprite + "_" + (chromaFurniture.IsSmallFurni ? "32" : "64") + "_" + letter + "_" + chromaFurniture.RenderDirection + "_" + RenderFrame;
            return builtString;
        }


        public Bitmap BuildImage()
        {
            /*string imagePath = null;

            if (chromaFurniture.OverrideRenderState != -1)
            {
                string letter = FileUtil.NumericLetter(AnimationLayer);
                var builtString = chromaFurniture.FurnitureClass + "_" + (chromaFurniture.IsSmallFurni ? "32" : "64") + "_" + letter + "_" + chromaFurniture.RenderDirection + "_" + chromaFurniture.OverrideRenderState;
                imagePath = FileUtil.SolveFile(chromaFurniture.OutputDirectory, builtString);
            }

            if (imagePath == null)
            {
                string letter = FileUtil.NumericLetter(AnimationLayer);
                var builtString = chromaFurniture.FurnitureClass + "_" + (chromaFurniture.IsSmallFurni ? "32" : "64") + "_" + letter + "_" + chromaFurniture.RenderDirection + "_" + chromaFurniture.RenderState;
                imagePath = FileUtil.SolveFile(chromaFurniture.OutputDirectory, builtString);
            }


            return new Bitmap(imagePath);//Image.FromFile(GetImagePath());
            */
            return null;
        }

        public string GetImagePath()
        {
            return FileUtil.SolveFile(chromaFurniture.OutputDirectory, BuildAssetName());
        }
    }
}