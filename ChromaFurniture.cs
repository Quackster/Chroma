using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace Chroma
{
    public class ChromaFurniture
    {
        private string fileName;
        public string OutputDirectory;
        public string FurnitureClass;
        public bool IsSmallFurni;

        public int RenderState;
        public int RenderDirection;

        public int ColourId;

        public List<ChromaAsset> Assets;

        public int CANVAS_WIDTH = 500;
        private Image CANVAS;
        public int CANVAS_HEIGHT = 500;

        public string CANVAS_PICTURE = null;//"bg.png";

        private static readonly string FFDEC_PATH = "C:\\Program Files (x86)\\FFDec\\ffdec.exe";

        //public List<ChromaAsset> BuildQueue;

        //private Dictionary<int, ChromaFrame> FrameSettings;
        //public int OverrideRenderState;
        //private bool FoundAnimations;

        public ChromaFurniture(string fileName, string outputDirectory, bool IsSmallFurni, int renderState, int renderDirection, int colourId = -1)
        {
            this.fileName = fileName;
            this.OutputDirectory = outputDirectory;
            this.FurnitureClass = Path.GetFileNameWithoutExtension(fileName);
            this.IsSmallFurni = IsSmallFurni;
            this.Assets = new List<ChromaAsset>();
            this.RenderState = renderState;
            this.RenderDirection = renderDirection;
            this.ColourId = colourId;
            //this.FrameSettings = new Dictionary<int, ChromaFrame>();
            //this.OverrideRenderState = overrideRenderState;
        }

        public void Run(string fileName = null)
        {
            //LocateRenderSettings();
            //LocateFrameSettings();
            //LocateFrameCoordinates();

            if (Directory.Exists(OutputDirectory))
                Directory.Delete(OutputDirectory, true);

            Directory.CreateDirectory(OutputDirectory);

            if (CANVAS_PICTURE != null)
            {
                CANVAS = Image.Load(CANVAS_PICTURE);//new Image<Rgba32>(CANVAS_HEIGHT, CANVAS_WIDTH, colour);

                CANVAS_HEIGHT = CANVAS.Height;
                CANVAS_WIDTH = CANVAS.Width;
            }

            var outputDirectory = new FileInfo(OutputDirectory).FullName;
            ExtractAssets(FFDEC_PATH);

            // Copy duplicate assets into their own pictures
            var swfmillPath = Path.Combine(OutputDirectory, "swfmill.xml");
            RunSwfmill(swfmillPath);
            CopyDuplicateImages(swfmillPath);

            GenerateAssets();
            CreateBuildQueue();
            BuildImage(fileName);
        }

        private void CopyDuplicateImages(string swfmillPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(swfmillPath);

            var nodes = xmlDoc.SelectNodes("//swf/Header/tags/SymbolClass/symbols/Symbol");

            for (int i = 0; i < nodes.Count; i++)
            {
                var symbol = nodes.Item(i);

                if (symbol == null)
                {
                    continue;
                }

                string objectID = symbol.Attributes.GetNamedItem("objectID").InnerText;
                string name = symbol.Attributes.GetNamedItem("name").InnerText;

                foreach (var file in Directory.GetFiles(Path.Combine(OutputDirectory, "images"), "*"))
                {
                    var fileName = Path.GetFileName(file);

                    if (fileName.StartsWith(objectID + "_"))
                    {
                        try
                        {
                            var newFile = name.Replace(Path.GetFileNameWithoutExtension(fileName) + "_", "") + ".png";
                            File.Copy(file, Path.Combine(OutputDirectory, "images", newFile));
                        }
                        catch { }
                    }
                }
            }
        }

        public void ExtractAssets(string ffdecPath)
        {
            var p = new Process();
            p.StartInfo.FileName = ffdecPath;
            p.StartInfo.Arguments = string.Format("-export \"binaryData,image\" \"{0}\" \"{1}\"", OutputDirectory, fileName);
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
        }


        private void RunSwfmill(string swfmillPath)
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "swfmill\\swfmill.exe");
            p.StartInfo.Arguments = "swf2xml \"" + fileName + "\" \"" + swfmillPath + "\"";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
        }

        private void GenerateAssets()
        {
            var xmlData = FileUtil.SolveXmlFile(OutputDirectory, "assets");

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

                //if (imageName.Contains("_sd_"))
                //    continue;

                if (asset.Attributes.GetNamedItem("source") != null)
                {
                    var newChromaAsset = new ChromaAsset(this, X, Y, asset.Attributes.GetNamedItem("source").InnerText, imageName);
                    CreateAsset(newChromaAsset, asset);
                }
                else
                {
                    var chromaAsset = new ChromaAsset(this, X, Y, null, imageName);
                    CreateAsset(chromaAsset, asset);
                }
            }
        }

        private void CreateAsset(ChromaAsset chromaAsset, XmlNode node)
        {
            if (Assets.Count(x => x.imageName == chromaAsset.imageName) == 0)
            {
                chromaAsset.flipH = (node.Attributes.GetNamedItem("flipH") != null && node.Attributes.GetNamedItem("flipH").InnerText == "1");
                Assets.Add(chromaAsset);

                if (chromaAsset.sourceImage != null)
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
                chromaAsset.Z += (chromaAsset.Layer * 1000);
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

            candidates = candidates.OrderBy(x => x.Z).ToList();
            return candidates;
        }

        private void BuildImage(string fileName = null)
        {
            var buildQueue = CreateBuildQueue();

            if (buildQueue == null)
                return;

            Rgba32[] cropColours = { Color.FromRgb(254,254,254) };// new Rgba32[] { Color.BlueViolet };//Color.FromRgb(142, 142, 90), Color.FromRgb(152, 152, 101) };//Color.Black;

            var canvasColour = Color.FromRgb(254, 254, 254);
            var canvas = CANVAS != null ? CANVAS : new Image<Rgba32>(CANVAS_HEIGHT, CANVAS_WIDTH, canvasColour);

            foreach (var asset in buildQueue)
            {
                var image = Image.Load<Rgba32>(asset.GetImagePath());

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
                        ctx.Opacity(0.1f);
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
                    ctx.DrawImage(image, new Point(canvas.Width - asset.ImageX, canvas.Height - asset.ImageY), graphicsOptions);
                });
            }

            if (cropColours != null && cropColours.Length > 0)
            {
                canvas.Save(GetFileName(fileName) + "-temp.png");

                // Crop the image
                System.Drawing.Bitmap tempBitmap = new System.Drawing.Bitmap(GetFileName(fileName) + "-temp.png");
                System.Drawing.Bitmap croppedBitmap = ImageUtil.TrimBitmap(tempBitmap, cropColours);


                croppedBitmap.Save(GetFileName(fileName) + ".png", System.Drawing.Imaging.ImageFormat.Png);
                croppedBitmap.Dispose();

                tempBitmap.Dispose();
                File.Delete(GetFileName(fileName) + "-temp.png");
            }
            else
            {
                canvas.Save(GetFileName(fileName) + ".png");
            }

            canvas.Dispose();
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

        private string GetFileName(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                string name = (IsSmallFurni ? "s_" : "") + FurnitureClass + "_" + RenderDirection + "_" + RenderState;

                if (this.ColourId > -1 && this.Assets.Count(x => x.ColourCode != null) > 0)
                {
                    name += "_colour" + this.ColourId;
                }

                return name;

            } else
            {
                return fileName;
            }
        }
    }
}
 