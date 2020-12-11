using System.Collections.Generic;
using System.Xml.Serialization;

namespace Extractor.Xml
{
    [XmlRoot("graphics")]
    public class Graphics
    {
        [XmlElement("visualization")]
        public List<Visualization> Visualizations;
    }


    [XmlRoot("visualizationData")]
    public class VisualizationData
    {
        [XmlAttribute("type")]
        public string Type;
        [XmlElement("graphics", IsNullable = true)]
        public Graphics Graphics;
        [XmlElement("visualization", IsNullable = true)]
        public List<Visualization> Visualizations;
    }

    public class Visualization
    {
        [XmlAttribute("size")]
        public int Size;
        [XmlAttribute("layerCount")]
        public int LayerCount;
        [XmlAttribute("angle")]
        public int Angle;

        [XmlArray("layers"), XmlArrayItem("layer")]
        public List<Layer> Layers;
        [XmlArray("directions"), XmlArrayItem("direction")]
        public List<Direction> Directions;
        [XmlArray("colors"), XmlArrayItem("color")]
        public List<Color> Colors;
    }

    public class Layer
    {
        [XmlAttribute("id")]
        public int Id;
        [XmlAttribute("z")]
        public int Z;
    }

    public class Direction
    {
        [XmlAttribute("id")]
        public int Id;
    }

    public class Color
    {
        [XmlAttribute("id")]
        public int Id;
        [XmlElement("colorLayer")]
        public ColorLayer ColorLayer;
    }

    public class ColorLayer
    {
        [XmlAttribute("id")]
        public int Id;
        [XmlAttribute("color")]
        public string Color;
    }
}
