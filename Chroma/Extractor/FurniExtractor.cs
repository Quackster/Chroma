using Flazzy;
using Flazzy.ABC;
using Flazzy.ABC.AVM2;
using Flazzy.ABC.AVM2.Instructions;
using Flazzy.IO;
using Flazzy.Tags;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Extractor
{
    public class FurniExtractor
    {
        public static bool Parse(string file)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                //if (Directory.Exists(@"furni_export\" + fileName))
                //    return false;

                var flash = new ShockwaveFlash(file);
                flash.Disassemble();

                var symbolClass = flash.Tags.Where(t => t.Kind == TagKind.SymbolClass).Cast<SymbolClassTag>().First();
                var imageTags = flash.Tags.Where(t => t.Kind == TagKind.DefineBitsLossless2).Cast<DefineBitsLossless2Tag>();
                var dataTags = flash.Tags.Where(t => t.Kind == TagKind.DefineBinaryData).Cast<DefineBinaryDataTag>();

                var furni = new Json.JsonFurniData();
                furni.visualization = new Json.Visualizations();
                furni.logic = new Json.Logic();
                furni.assets = new Dictionary<string, Json.Asset>();

                foreach (var data in dataTags)
                {
                    var name = symbolClass.Names[symbolClass.Ids.IndexOf(data.Id)];
                    var type = name.Split('_')[name.Split('_').Length - 1];
                    var txt = Encoding.Default.GetString(data.Data);

                    var xml = new XmlDocument();
                    xml.LoadXml(txt);
                    dynamic x = JsonConvert.DeserializeObject(JsonConvert.SerializeXmlNode(xml.DocumentElement));

                    if (!Directory.Exists(@"furni_export/" + fileName + "/xml"))
                        Directory.CreateDirectory(@"furni_export/" + fileName + "/xml");

                    File.WriteAllText(@"furni_export/" + fileName + "/xml/" + type + ".xml", txt);

                    switch (type)
                    {
                        case "index":
                            furni.type = x.@object["@type"];
                            furni.visualization.type = x.@object["@visualization"];
                            furni.logic.type = x.@object["@logic"];
                            break;
                    }
                }
                        /*case "logic":
                            furni.logic.directions = new List<string>();
                            furni.logic.dimensions = new Dictionary<string, string>();

                            foreach (var dir in x.objectData.model.directions.direction)
                                furni.logic.directions.Add("" + dir["@id"]);
                            foreach (var dim in x.objectData.model.dimensions)
                                furni.logic.dimensions.Add(dim.Name.Replace("@", ""), "" + dim.Value);
                            break;
                        case "assets":
                            foreach (var asset in x.assets.asset)
                            {
                                var jAsset = new Json.Asset() { name = asset["@name"], x = asset["@x"], y = asset["@y"] };
                                if (asset["@flipH"] != null)
                                    jAsset.flipH = asset["@flipH"];
                                if (asset["@source"] != null)
                                    jAsset.source = asset["@source"];
                                furni.assets.Add((string)asset["@name"], jAsset);
                            }
                            break;
                        case "visualization":
                            JObject z = x.visualizationData;
                            XmlSerializer serializer = null;
                            serializer = new XmlSerializer(typeof(Xml.VisualizationData));

                            using (TextReader reader = new StreamReader(new MemoryStream(data.Data), Encoding.Default))
                            {
                                Xml.VisualizationData result = new Xml.VisualizationData();

                                if (z["graphics"] != null)
                                {
                                    var deser = (Xml.VisualizationData)serializer.Deserialize(reader);
                                    result.Type = deser.Type;
                                    result.Visualizations = deser.Graphics.Visualizations;
                                }
                                else
                                {
                                    result = (Xml.VisualizationData)serializer.Deserialize(reader);
                                }

                                foreach (var viz in result.Visualizations)
                                {
                                    var jViz = new Json.Visualization();
                                    jViz.layers = new List<Json.Layer>();
                                    jViz.directions = new Dictionary<string, List<string>>();
                                    jViz.colors = new Dictionary<string, List<Json.Color>>();

                                    jViz.size = "" + viz.Size;
                                    jViz.layerCount = "" + viz.LayerCount;
                                    jViz.angle = "" + viz.Angle;
                                    foreach (var lay in viz.Layers)
                                    {
                                        jViz.layers.Add(new Json.Layer()
                                        {
                                            id = "" + lay.Id,
                                            z = "" + lay.Z
                                        });
                                    }

                                    foreach (var dir in viz.Directions)
                                    {
                                        jViz.directions.Add("" + dir.Id, new List<string>());
                                    }

                                    foreach (var col in viz.Colors)
                                    {
                                        var color = new List<Json.Color>() { new Json.Color() { layerId = col.ColorLayer.Id, color = col.ColorLayer.Color } };
                                        jViz.colors.Add("" + col.Id, color);
                                    }

                                    switch (viz.Size)
                                    {
                                        case 1:
                                            furni.visualization.Size1 = jViz;
                                            break;
                                        case 32:
                                            furni.visualization.Size32 = jViz;
                                            break;
                                        case 64:
                                            furni.visualization.Size64 = jViz;
                                            break;
                                    }
                                }
                            }
                            break;
                    }*/


                if (!Directory.Exists(@"furni_export/" + fileName))
                    Directory.CreateDirectory(@"furni_export/" + fileName);

                foreach (var image in imageTags)
                {
                    var name = symbolClass.Names[symbolClass.Ids.IndexOf(image.Id)];
                    System.Drawing.Color[,] table = image.GetARGBMap();

                    int width = table.GetLength(0);
                    int height = table.GetLength(1);
                    using (var asset = new Image<Rgba32>(width, height))
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                System.Drawing.Color pixel = table[x, y];
                                asset[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, pixel.A);
                            }
                        }

                        name = name.Replace(furni.type + "_" + fileName, fileName);

                        using (var output = new StreamWriter(@"furni_export/" + fileName + "/" + name + ".png"))
                        {
                            asset.SaveAsPng(output.BaseStream);
                        }
                    }
                }


                var json = JsonConvert.SerializeObject(furni);
                File.WriteAllText(@"furni_export\" + furni.type + "\\furni.json", json);
                return true;
            }
            catch (Exception) { throw; }
        }
    }
}
