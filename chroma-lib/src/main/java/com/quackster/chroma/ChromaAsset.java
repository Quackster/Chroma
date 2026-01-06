package com.quackster.chroma;

import com.quackster.chroma.util.FileUtil;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import javax.imageio.ImageIO;
import java.awt.geom.AffineTransform;
import java.awt.image.AffineTransformOp;
import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;

/**
 * Represents a single asset/sprite in a Chroma furniture item
 */
public class ChromaAsset {
    private int relativeX;
    private int relativeY;
    private int imageX;
    private int imageY;
    private int z = -1;
    private String sourceImage;
    private String imageName;
    private boolean flipH;
    private int layer;
    private int direction;
    private int frame;
    private boolean isSmall;
    private ChromaFurniture chromaFurniture;
    private String ink;
    private boolean ignoreMouse;
    private boolean shadow;
    private String colourCode;
    private int alpha = -1;

    public ChromaAsset(ChromaFurniture chromaFurniture, int x, int y, String sourceImage, String imageName) {
        this.chromaFurniture = chromaFurniture;
        this.relativeX = x;
        this.relativeY = y;
        this.imageX = x;
        this.imageY = y;
        this.sourceImage = sourceImage;
        this.imageName = imageName;
    }

    public boolean parse() {
        try {
            String dataName = imageName.replace(chromaFurniture.getSprite() + "_", "");
            String[] data = dataName.split("_");

            isSmall = data[0].equals("32");
            layer = (data[1].toUpperCase().charAt(0) - 64) - 1;
            direction = chromaFurniture.isIcon() ? 0 : Integer.parseInt(data[2]);
            frame = chromaFurniture.isIcon() ? 0 : Integer.parseInt(data[3]);

            Document xmlData = FileUtil.solveXmlFile(chromaFurniture.getXmlDirectory(), "visualization");
            
            if (xmlData == null) {
                return false;
            }

            String size = chromaFurniture.isSmallFurni() ? "32" : "64";
            NodeList visualisationLayers = xmlData.getDocumentElement().getElementsByTagName("layer");
            
            // Try to find layers in the standard location first
            visualisationLayers = findLayersInVisualization(xmlData, size, null);
            
            // If not found, try direction-specific layers
            if (visualisationLayers == null || visualisationLayers.getLength() == 0) {
                visualisationLayers = findLayersInVisualization(xmlData, size, String.valueOf(chromaFurniture.getRenderDirection()));
            }

            if (visualisationLayers != null) {
                for (int i = 0; i < visualisationLayers.getLength(); i++) {
                    Node layerNode = visualisationLayers.item(i);
                    Node idAttr = layerNode.getAttributes().getNamedItem("id");
                    
                    if (idAttr == null) continue;
                    
                    int animationLayer = Integer.parseInt(idAttr.getNodeValue());

                    if (animationLayer == this.layer) {
                        Node inkAttr = layerNode.getAttributes().getNamedItem("ink");
                        if (inkAttr != null) {
                            ink = inkAttr.getNodeValue();
                        }

                        Node ignoreMouseAttr = layerNode.getAttributes().getNamedItem("ignoreMouse");
                        if (ignoreMouseAttr != null) {
                            ignoreMouse = ignoreMouseAttr.getNodeValue().equals("1");
                        }

                        Node zAttr = layerNode.getAttributes().getNamedItem("z");
                        if (zAttr != null) {
                            z = Integer.parseInt(zAttr.getNodeValue());
                        }

                        Node alphaAttr = layerNode.getAttributes().getNamedItem("alpha");
                        if (alphaAttr != null) {
                            alpha = Integer.parseInt(alphaAttr.getNodeValue());
                        }
                    }
                }
            }

            if (z == -1) {
                z = layer;
            }

            if (chromaFurniture.getColourId() > -1) {
                NodeList colorLayers = findColorLayers(xmlData, size, chromaFurniture.getColourId(), layer);
                
                if (colorLayers != null && colorLayers.getLength() > 0) {
                    Node colorAttr = colorLayers.item(0).getAttributes().getNamedItem("color");
                    if (colorAttr != null) {
                        colourCode = colorAttr.getNodeValue();
                    }
                }
            }

        } catch (Exception e) {
            System.err.println("Error parsing asset: " + e.getMessage());
            e.printStackTrace();
            return false;
        }

        return true;
    }

    private NodeList findLayersInVisualization(Document xmlData, String size, String direction) {
        try {
            String xpath = "//visualizationData/visualization[@size='" + size + "']";
            if (direction != null) {
                xpath += "/directions/direction[@id='" + direction + "']";
            }
            xpath += "/layers/layer";
            
            // Simple XPath alternative since we're not using XPath library
            NodeList visualizations = xmlData.getElementsByTagName("visualization");
            for (int i = 0; i < visualizations.getLength(); i++) {
                Node viz = visualizations.item(i);
                Node sizeAttr = viz.getAttributes().getNamedItem("size");
                
                if (sizeAttr != null && sizeAttr.getNodeValue().equals(size)) {
                    if (direction == null) {
                        NodeList layersNodes = ((org.w3c.dom.Element) viz).getElementsByTagName("layers");
                        if (layersNodes.getLength() > 0) {
                            return ((org.w3c.dom.Element) layersNodes.item(0)).getElementsByTagName("layer");
                        }
                    } else {
                        NodeList directions = ((org.w3c.dom.Element) viz).getElementsByTagName("direction");
                        for (int j = 0; j < directions.getLength(); j++) {
                            Node dir = directions.item(j);
                            Node idAttr = dir.getAttributes().getNamedItem("id");
                            if (idAttr != null && idAttr.getNodeValue().equals(direction)) {
                                NodeList layersNodes = ((org.w3c.dom.Element) dir).getElementsByTagName("layers");
                                if (layersNodes.getLength() > 0) {
                                    return ((org.w3c.dom.Element) layersNodes.item(0)).getElementsByTagName("layer");
                                }
                            }
                        }
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    private NodeList findColorLayers(Document xmlData, String size, int colorId, int layerId) {
        try {
            NodeList visualizations = xmlData.getElementsByTagName("visualization");
            for (int i = 0; i < visualizations.getLength(); i++) {
                Node viz = visualizations.item(i);
                Node sizeAttr = viz.getAttributes().getNamedItem("size");
                
                if (sizeAttr != null && sizeAttr.getNodeValue().equals(size)) {
                    NodeList colorsNodes = ((org.w3c.dom.Element) viz).getElementsByTagName("colors");
                    if (colorsNodes.getLength() > 0) {
                        NodeList colors = ((org.w3c.dom.Element) colorsNodes.item(0)).getElementsByTagName("color");
                        for (int j = 0; j < colors.getLength(); j++) {
                            Node color = colors.item(j);
                            Node idAttr = color.getAttributes().getNamedItem("id");
                            if (idAttr != null && idAttr.getNodeValue().equals(String.valueOf(colorId))) {
                                NodeList colorLayers = ((org.w3c.dom.Element) color).getElementsByTagName("colorLayer");
                                for (int k = 0; k < colorLayers.getLength(); k++) {
                                    Node colorLayer = colorLayers.item(k);
                                    Node layerIdAttr = colorLayer.getAttributes().getNamedItem("id");
                                    if (layerIdAttr != null && layerIdAttr.getNodeValue().equals(String.valueOf(layerId))) {
                                        // Return a NodeList with just this element
                                        return new NodeList() {
                                            @Override
                                            public Node item(int index) {
                                                return index == 0 ? colorLayer : null;
                                            }
                                            
                                            @Override
                                            public int getLength() {
                                                return 1;
                                            }
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    public void generateImage() {
        String file = FileUtil.solveFile(chromaFurniture.getOutputDirectory(), sourceImage);

        if (file != null) {
            String newName = imageName + ".png";
            String newPath = new File(chromaFurniture.getOutputDirectory(), newName).getPath();

            File newFile = new File(newPath);
            if (newFile.exists() && flipH) {
                try {
                    BufferedImage bitmap = ImageIO.read(newFile);
                    relativeX = bitmap.getWidth() - relativeX;
                    imageX = relativeX;
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    public String getImagePath() {
        return FileUtil.solveFile(chromaFurniture.getOutputDirectory(), imageName);
    }

    // Getters and setters
    public int getRelativeX() { return relativeX; }
    public void setRelativeX(int relativeX) { this.relativeX = relativeX; }
    
    public int getRelativeY() { return relativeY; }
    public void setRelativeY(int relativeY) { this.relativeY = relativeY; }
    
    public int getImageX() { return imageX; }
    public void setImageX(int imageX) { this.imageX = imageX; }
    
    public int getImageY() { return imageY; }
    public void setImageY(int imageY) { this.imageY = imageY; }
    
    public int getZ() { return z; }
    public void setZ(int z) { this.z = z; }
    
    public String getSourceImage() { return sourceImage; }
    public void setSourceImage(String sourceImage) { this.sourceImage = sourceImage; }
    
    public String getImageName() { return imageName; }
    public void setImageName(String imageName) { this.imageName = imageName; }
    
    public boolean isFlipH() { return flipH; }
    public void setFlipH(boolean flipH) { this.flipH = flipH; }
    
    public int getLayer() { return layer; }
    public void setLayer(int layer) { this.layer = layer; }
    
    public int getDirection() { return direction; }
    public void setDirection(int direction) { this.direction = direction; }
    
    public int getFrame() { return frame; }
    public void setFrame(int frame) { this.frame = frame; }
    
    public boolean isSmall() { return isSmall; }
    public void setSmall(boolean small) { isSmall = small; }
    
    public String getInk() { return ink; }
    public void setInk(String ink) { this.ink = ink; }
    
    public boolean isIgnoreMouse() { return ignoreMouse; }
    public void setIgnoreMouse(boolean ignoreMouse) { this.ignoreMouse = ignoreMouse; }
    
    public boolean isShadow() { return shadow; }
    public void setShadow(boolean shadow) { this.shadow = shadow; }
    
    public String getColourCode() { return colourCode; }
    public void setColourCode(String colourCode) { this.colourCode = colourCode; }
    
    public int getAlpha() { return alpha; }
    public void setAlpha(int alpha) { this.alpha = alpha; }
}
