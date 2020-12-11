using Extractor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chroma.Extensions;
using System.Drawing;
using Color = SixLabors.ImageSharp.Color;

namespace Chroma
{
    public class ChromaFurniture
    {
        private string fileName;
        private string outputFileName;
        public bool IsSmallFurni;

        public int RenderState;
        public int RenderDirection;

        public int ColourId;
        public string Sprite;
        public List<ChromaAsset> Assets;
        public Image<Rgba32> CANVAS;

        public int CANVAS_WIDTH = 500;
        public int CANVAS_HEIGHT = 500;
        public string CANVAS_PICTURE = null;//"bg.png";

        public string FurniData;
        private bool RenderShadows;

        //public List<ChromaAsset> BuildQueue;

        //private Dictionary<int, ChromaFrame> FrameSettings;
        //public int OverrideRenderState;
        //private bool FoundAnimations;

        public string OutputDirectory
        {
            get { return Path.Combine("furni_export", Sprite); }
        }

        public string XmlDirectory
        {
            get { return Path.Combine("furni_export", Sprite, "xml"); }
        }

        public ChromaFurniture(string inputFileName, bool IsSmallFurni, int renderState, int renderDirection, int colourId = -1, bool RenderShadows = false)
        {
            this.fileName = inputFileName;
            this.IsSmallFurni = IsSmallFurni;
            this.Assets = new List<ChromaAsset>();
            this.RenderState = renderState;
            this.RenderDirection = renderDirection;
            this.ColourId = colourId;
            this.Sprite = Path.GetFileNameWithoutExtension(inputFileName);
            this.outputFileName = this.GetFileName();
            this.FurniData = Path.Combine("furni_export/" +  Path.GetFileNameWithoutExtension(inputFileName) + "/furni.json");
            this.RenderShadows = RenderShadows;
        }

        public string Run()
        {
            FurniExtractor.Parse(this.fileName);

            if (CANVAS_PICTURE != null)
            {
                CANVAS = SixLabors.ImageSharp.Image.Load<Rgba32>(CANVAS_PICTURE);//new Image<Rgba32>(CANVAS_HEIGHT, CANVAS_WIDTH, colour);

                CANVAS_HEIGHT = CANVAS.Height;
                CANVAS_WIDTH = CANVAS.Width;
            }
;
            GenerateAssets();
            CreateBuildQueue();

            this.outputFileName = this.GetFileName();
            return this.outputFileName;
        }

        private void GenerateAssets(bool createFiles = true)
        {
            var xmlData = FileUtil.SolveXmlFile(XmlDirectory, "assets");

            if (xmlData == null)
            {
                return;
            }

            XmlNodeList assets = xmlData.SelectNodes("//assets/asset");

            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets.Item(i);

                var X = int.Parse(asset.Attributes.GetNamedItem("x").InnerText);
                var Y = int.Parse(asset.Attributes.GetNamedItem("y").InnerText);

                string imageName = asset.Attributes.GetNamedItem("name").InnerText;

                if (imageName.Contains("_icon_"))
                    continue;

                if (imageName.Contains(".props") || imageName.StartsWith("s_" + this.Sprite))
                    continue;

                if (asset.Attributes.GetNamedItem("source") != null)
                {
                    var newChromaAsset = new ChromaAsset(this, X, Y, asset.Attributes.GetNamedItem("source").InnerText, imageName);
                    CreateAsset(newChromaAsset, asset, createFiles);
                }
                else
                {
                    var chromaAsset = new ChromaAsset(this, X, Y, null, imageName);
                    CreateAsset(chromaAsset, asset, createFiles);
                }
            }
        }

        private void CreateAsset(ChromaAsset chromaAsset, XmlNode node, bool createFiles)
        {
            if (!chromaAsset.Parse())
                return;

            if (Assets.Count(x => x.imageName == chromaAsset.imageName) == 0)
            {
                chromaAsset.flipH = (node.Attributes.GetNamedItem("flipH") != null && node.Attributes.GetNamedItem("flipH").InnerText == "1");
                Assets.Add(chromaAsset);

                if (chromaAsset.sourceImage != null && createFiles)
                {
                    chromaAsset.GenerateImage();
                }

                chromaAsset.ImageX = chromaAsset.ImageX + (CANVAS_WIDTH / 2);// 32;
                chromaAsset.ImageY = chromaAsset.ImageY + (CANVAS_HEIGHT / 2);// 25;
            }

            if (chromaAsset.imageName.Contains("_sd_"))
            {
                chromaAsset.Shadow = true;
                chromaAsset.Z = int.MinValue;
            } else
            {
                chromaAsset.Z += chromaAsset.Layer;
            }
        }

        private List<ChromaAsset> CreateBuildQueue()
        {
            var compulsoryFrames = new List<ChromaAsset>(Assets);

            compulsoryFrames = compulsoryFrames.Where(x => x.IsSmall == IsSmallFurni).ToList();
            compulsoryFrames = compulsoryFrames.Where(x => x.Frame == 0).ToList();

            var candidates = new List<ChromaAsset>(Assets);
            candidates = candidates.Where(x => x.IsSmall == IsSmallFurni).ToList();

            var validDirections = candidates.Where(x => x.Direction == RenderDirection).ToList();

            if (validDirections.Count == 0)
            {
                RenderDirection = 0;
                validDirections = candidates.Where(x => x.Direction == RenderDirection).ToList();
            }

            if (validDirections.Count == 0)
            {
                RenderDirection = 2;
                validDirections = candidates.Where(x => x.Direction == RenderDirection).ToList();
            }

            if (validDirections.Count == 0)
            {
                RenderDirection = 4;
                validDirections = candidates.Where(x => x.Direction == RenderDirection).ToList();
            }

            if (validDirections.Count == 0)
            {
                RenderDirection = 6;
                validDirections = candidates.Where(x => x.Direction == RenderDirection).ToList();
            }

            // If for some reason the frame/state doesn't exist
            candidates = validDirections.Where(x => x.Frame == RenderState).ToList();

            if (candidates.ToList().Count == 0)
            {
                RenderState = 0; // Reset state
                candidates = new List<ChromaAsset>(Assets);
                candidates = candidates.Where(x => x.IsSmall == IsSmallFurni).ToList();
                candidates = candidates.Where(x => x.Direction == RenderDirection).ToList();
                candidates = candidates.Where(x => x.Frame == 0).ToList();
            }

            // Select the missing frames ordered by direction
            if (compulsoryFrames.Count > 0)
            {
                compulsoryFrames = compulsoryFrames.Where(x => x.Direction == RenderDirection).ToList();
            }

            // Add missing frames if they don't exist
            foreach (var frame in compulsoryFrames)
            {
                if (candidates.Count(x => x.Layer == frame.Layer) > 0)
                {
                    continue;
                }

                var nextFrame = compulsoryFrames.Where(x => x.Layer == frame.Layer && x.Direction == frame.Direction && x.Frame == 0).FirstOrDefault();

                if (nextFrame != null)
                {
                    candidates.Add(nextFrame);
                }
            }

            if (!this.RenderShadows)
            {
                //candidates = candidates.Where(x => !x.Shadow).ToList();
            }

            candidates = candidates.OrderBy(x => x.Z).ToList();
            return candidates;
        }

        public byte[] CreateImage()
        {
            var buildQueue = CreateBuildQueue();

            if (buildQueue == null)
                return null;

            Rgba32[] cropColours = { Color.FromRgb(254, 254, 254) };// new Rgba32[] { Color.BlueViolet };//Color.FromRgb(142, 142, 90), Color.FromRgb(152, 152, 101) };//Color.Black;
            Color canvasColour = Color.FromRgb(254, 254, 254);

            using (var canvas = CANVAS != null ? CANVAS : new Image<Rgba32>(CANVAS_HEIGHT, CANVAS_WIDTH, canvasColour))
            {
                foreach (var asset in buildQueue)
                {
                    var image = SixLabors.ImageSharp.Image.Load<Rgba32>(asset.GetImagePath());

                    if (asset.Alpha != -1)
                    {
                        TintImage(image, "FFFFFF", (byte)asset.Alpha);
                    }

                    if (asset.ColourCode != null)
                    {
                        TintImage(image, asset.ColourCode, 255);
                    }

                    if (asset.Shadow)
                    {
                        image.Mutate(ctx =>
                        {
                            ctx.Opacity(0.2f);
                        });
                    }

                    var graphicsOptions = new GraphicsOptions();

                    if ((asset.Ink == "ADD" || asset.Ink == "33"))
                    {
                        graphicsOptions.ColorBlendingMode = PixelColorBlendingMode.Add;
                    }
                    else
                    {
                        graphicsOptions.ColorBlendingMode = PixelColorBlendingMode.Normal;
                    }

                    canvas.Mutate(ctx =>
                    {
                        ctx.DrawImage(image, new SixLabors.ImageSharp.Point(canvas.Width - asset.ImageX, canvas.Height - asset.ImageY), graphicsOptions);
                    });
                }


                using (Bitmap tempBitmap = canvas.ToBitmap())
                {
                    if (cropColours != null && cropColours.Length > 0)
                    {
                        var temp = canvas.ToBitmap();

                        // Crop the image
                        using (Bitmap croppedBitmap = ImageUtil.TrimBitmap(tempBitmap, cropColours))
                        {
                            return RenderImage(croppedBitmap);
                        }

                    }
                    else
                    {
                        return RenderImage(tempBitmap);
                    }
                }
            }
        }

        private byte[] RenderImage(Bitmap croppedBitmap)
        {
            return croppedBitmap.ToByteArray();
        }

        private void TintImage(Image<Rgba32> image, string colourCode, byte alpha)
        {
            var rgb = HexToColor(colourCode);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var current = image[x, y];

                    if (current.A > 0)
                    {
                        current.R = (byte)(rgb.R * current.R / 255);
                        current.G = (byte)(rgb.G * current.G / 255);
                        current.B = (byte)(rgb.B * current.B / 255);
                        current.A = alpha;
                    }

                    image[x, y] = current;
                }
            }
        }

        public static System.Drawing.Color HexToColor(string hexString)
        {
           return System.Drawing.ColorTranslator.FromHtml("#" + hexString);
        }

        private string GetFileName()
        {
            string name = (IsSmallFurni ? "s_" : "") + Sprite + "_" + RenderDirection + "_" + RenderState;

            if (this.ColourId > -1 && this.Assets.Count(x => x.ColourCode != null) > 0)
            {
                name += "_colour" + this.ColourId;
            }

            return name + ".png";
        }
    }
}
 