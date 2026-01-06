package com.quackster.chroma;

import com.google.gson.Gson;
import com.quackster.chroma.extractor.FurniExtractor;
import com.quackster.chroma.util.FileUtil;
import com.quackster.chroma.util.ImageUtil;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import javax.imageio.ImageIO;
import java.awt.*;
import java.awt.image.BufferedImage;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.nio.file.Paths;
import java.util.List;
import java.util.*;
import java.util.stream.Collectors;

/**
 * Main class for rendering Habbo furniture from SWF files
 */
public class ChromaFurniture {
    private String fileName;
    private String outputFileName;
    private boolean isSmallFurni;
    
    private int renderState;
    private int renderDirection;
    private int colourId;
    private String sprite;
    private List<ChromaAsset> assets;
    private BufferedImage drawingCanvas;
    
    private static final int CANVAS_WIDTH = 1200;
    private static final int CANVAS_HEIGHT = 1200;
    private static final String CANVAS_PICTURE = "bg.png";
    
    private String furniData;
    private boolean renderShadows;
    private boolean renderBackground;
    private String renderCanvasColour;
    private boolean cropImage;
    private boolean isIcon;
    private int animationCount;
    private TreeMap<Integer, ChromaAnimation> animations;
    private int highestAnimationLayer;
    private int maxStates;

    public ChromaFurniture(String inputFileName, boolean isSmallFurni, int renderState, 
                          int renderDirection, int colourId, boolean renderShadows,
                          boolean renderBackground, String renderCanvasColour, 
                          boolean cropImage, boolean renderIcon) {
        this.fileName = inputFileName;
        this.isSmallFurni = isSmallFurni;
        this.assets = new ArrayList<>();
        this.renderState = renderState;
        this.renderDirection = renderDirection;
        this.colourId = colourId;
        this.sprite = getFileNameWithoutExtension(inputFileName);
        this.outputFileName = getFileName();
        this.furniData = Paths.get("furni_export", getFileNameWithoutExtension(inputFileName), "furni.json").toString();
        this.renderShadows = renderShadows;
        this.renderBackground = renderBackground;
        this.renderCanvasColour = renderCanvasColour;
        this.cropImage = cropImage;
        this.isIcon = renderIcon;
        this.animations = new TreeMap<>();
    }

    public String run() {
        FurniExtractor.parse(this.fileName);
        
        if (this.renderBackground) {
            try {
                File bgFile = new File(CANVAS_PICTURE);
                if (bgFile.exists()) {
                    drawingCanvas = ImageIO.read(bgFile);
                } else {
                    drawingCanvas = new BufferedImage(CANVAS_WIDTH, CANVAS_HEIGHT, BufferedImage.TYPE_INT_ARGB);
                }
            } catch (IOException e) {
                drawingCanvas = new BufferedImage(CANVAS_WIDTH, CANVAS_HEIGHT, BufferedImage.TYPE_INT_ARGB);
            }
        } else {
            drawingCanvas = new BufferedImage(CANVAS_WIDTH, CANVAS_HEIGHT, BufferedImage.TYPE_INT_ARGB);
            Graphics2D g = drawingCanvas.createGraphics();
            g.setColor(hexToColor(this.renderCanvasColour));
            g.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
            g.dispose();
        }
        
        generateAnimations();
        generateAssets();
        
        createBuildQueue();
        
        this.outputFileName = getFileName();
        return this.outputFileName;
    }

    private void generateAnimations() {
        Document xmlData = FileUtil.solveXmlFile(getXmlDirectory(), "visualization");
        
        this.animationCount = 0;
        this.animations = new TreeMap<>();
        
        if (xmlData == null) {
            return;
        }
        
        String size = isSmallFurni ? "32" : "64";
        NodeList frames = findFrames(xmlData, size);
        
        this.highestAnimationLayer = 0;
        
        if (frames != null) {
            for (int i = 0; i < frames.getLength(); i++) {
                Node frame = frames.item(i);
                
                Node animationLayerNode = frame.getParentNode().getParentNode();
                Node layerIdAttr = animationLayerNode.getAttributes().getNamedItem("id");
                
                if (layerIdAttr == null) continue;
                
                int letterPosition = Integer.parseInt(layerIdAttr.getNodeValue());
                
                if (letterPosition < 0 || letterPosition >= 26) {
                    continue;
                }
                
                int animationLetter = letterPosition;
                highestAnimationLayer = Math.max(highestAnimationLayer, letterPosition + 1);
                
                Node animationNode = animationLayerNode.getParentNode();
                Node animIdAttr = animationNode.getAttributes().getNamedItem("id");
                
                if (animIdAttr == null) continue;
                
                int animationId = Integer.parseInt(animIdAttr.getNodeValue());
                int castAnimationId = animationId + 1;
                
                if (castAnimationId > this.animationCount) {
                    this.animationCount = castAnimationId;
                }
                
                if (!this.animations.containsKey(animationLetter)) {
                    this.animations.put(animationLetter, new ChromaAnimation());
                }
                
                if (!this.animations.get(animationLetter).getStates().containsKey(animationId)) {
                    ChromaFrame frameClass = new ChromaFrame();
                    this.animations.get(animationLetter).getStates().put(animationId, frameClass);
                    
                    Node loopCountAttr = animationLayerNode.getAttributes().getNamedItem("loopCount");
                    if (loopCountAttr != null) {
                        frameClass.setLoop(Integer.parseInt(loopCountAttr.getNodeValue()));
                    }
                    
                    Node frameRepeatAttr = animationLayerNode.getAttributes().getNamedItem("frameRepeat");
                    if (frameRepeatAttr != null) {
                        frameClass.setFramesPerSecond(Integer.parseInt(frameRepeatAttr.getNodeValue()));
                    }
                }
                
                Node frameIdAttr = frame.getAttributes().getNamedItem("id");
                if (frameIdAttr != null) {
                    this.animations.get(animationLetter).getStates().get(animationId).getFrames().add(frameIdAttr.getNodeValue());
                }
            }
        }
    }

    private NodeList findFrames(Document xmlData, String size) {
        try {
            NodeList visualizations = xmlData.getElementsByTagName("visualization");
            for (int i = 0; i < visualizations.getLength(); i++) {
                Node viz = visualizations.item(i);
                Node sizeAttr = viz.getAttributes().getNamedItem("size");
                
                if (sizeAttr != null && sizeAttr.getNodeValue().equals(size)) {
                    NodeList animations = ((org.w3c.dom.Element) viz).getElementsByTagName("animation");
                    if (animations.getLength() > 0) {
                        // Collect all frame nodes
                        List<Node> frameNodes = new ArrayList<>();
                        for (int j = 0; j < animations.getLength(); j++) {
                            Node anim = animations.item(j);
                            NodeList animLayers = ((org.w3c.dom.Element) anim).getElementsByTagName("animationLayer");
                            for (int k = 0; k < animLayers.getLength(); k++) {
                                Node animLayer = animLayers.item(k);
                                NodeList frameSeqs = ((org.w3c.dom.Element) animLayer).getElementsByTagName("frameSequence");
                                for (int l = 0; l < frameSeqs.getLength(); l++) {
                                    Node frameSeq = frameSeqs.item(l);
                                    NodeList frames = ((org.w3c.dom.Element) frameSeq).getElementsByTagName("frame");
                                    for (int m = 0; m < frames.getLength(); m++) {
                                        frameNodes.add(frames.item(m));
                                    }
                                }
                            }
                        }
                        
                        if (!frameNodes.isEmpty()) {
                            return new NodeListWrapper(frameNodes);
                        }
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    private void generateAssets() {
        Document xmlData = FileUtil.solveXmlFile(getXmlDirectory(), "assets");
        
        if (xmlData == null) {
            return;
        }
        
        NodeList assetNodes = xmlData.getElementsByTagName("asset");
        
        for (int i = 0; i < assetNodes.getLength(); i++) {
            Node asset = assetNodes.item(i);
            
            Node xAttr = asset.getAttributes().getNamedItem("x");
            Node yAttr = asset.getAttributes().getNamedItem("y");
            Node nameAttr = asset.getAttributes().getNamedItem("name");
            
            if (xAttr == null || yAttr == null || nameAttr == null) continue;
            
            int x = Integer.parseInt(xAttr.getNodeValue());
            int y = Integer.parseInt(yAttr.getNodeValue());
            String imageName = nameAttr.getNodeValue();
            
            if (imageName.contains(".props") || imageName.startsWith("s_" + this.sprite)) {
                continue;
            }
            
            if (!isIcon) {
                if (imageName.contains("_icon_")) continue;
            } else {
                if (!imageName.contains("_icon_")) continue;
            }
            
            Node sourceAttr = asset.getAttributes().getNamedItem("source");
            ChromaAsset chromaAsset;
            
            if (sourceAttr != null) {
                chromaAsset = new ChromaAsset(this, x, y, sourceAttr.getNodeValue(), imageName);
            } else {
                chromaAsset = new ChromaAsset(this, x, y, null, imageName);
            }
            
            createAsset(chromaAsset, asset, true);
        }
        
        // Calculate max states
        this.maxStates = 0;
        Document visualization = FileUtil.solveXmlFile(getXmlDirectory(), "visualization");
        
        if (visualization != null) {
            NodeList animations = findAnimations(visualization, isSmallFurni ? "32" : "64");
            
            if (animations != null) {
                for (int i = 0; i < animations.getLength(); i++) {
                    Node animation = animations.item(i);
                    Node idAttr = animation.getAttributes().getNamedItem("id");
                    
                    if (idAttr != null) {
                        int state = Integer.parseInt(idAttr.getNodeValue());
                        if (state > maxStates) {
                            maxStates = state;
                        }
                    }
                }
            }
        }
        
        this.highestAnimationLayer = assets.stream()
            .filter(x -> !x.isShadow())
            .mapToInt(ChromaAsset::getLayer)
            .max()
            .orElse(0) + 1;
        
        // Fill in missing animation states
        for (int i = 0; i < highestAnimationLayer; i++) {
            if (!this.animations.containsKey(i)) {
                ChromaAnimation animation = new ChromaAnimation();
                this.animations.put(i, animation);
                
                for (int j = 0; j < this.animationCount; j++) {
                    if (!animation.getStates().containsKey(j)) {
                        ChromaFrame frame = new ChromaFrame();
                        frame.getFrames().add("0");
                        animation.getStates().put(j, frame);
                    }
                }
            }
        }
    }

    private NodeList findAnimations(Document xmlData, String size) {
        try {
            List<Node> animNodes = new ArrayList<>();
            NodeList visualizations = xmlData.getElementsByTagName("visualization");
            
            for (int i = 0; i < visualizations.getLength(); i++) {
                Node viz = visualizations.item(i);
                Node sizeAttr = viz.getAttributes().getNamedItem("size");
                
                if (sizeAttr != null && sizeAttr.getNodeValue().equals(size)) {
                    NodeList anims = ((org.w3c.dom.Element) viz).getElementsByTagName("animation");
                    for (int j = 0; j < anims.getLength(); j++) {
                        animNodes.add(anims.item(j));
                    }
                }
            }
            
            return animNodes.isEmpty() ? null : new NodeListWrapper(animNodes);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    private void createAsset(ChromaAsset chromaAsset, Node node, boolean createFiles) {
        if (!chromaAsset.parse()) {
            return;
        }
        
        boolean exists = assets.stream().anyMatch(x -> x.getImageName().equals(chromaAsset.getImageName()));
        
        if (!exists) {
            Node flipHAttr = node.getAttributes().getNamedItem("flipH");
            chromaAsset.setFlipH(flipHAttr != null && flipHAttr.getNodeValue().equals("1"));
            assets.add(chromaAsset);
            
            if (chromaAsset.getSourceImage() != null && createFiles) {
                chromaAsset.generateImage();
            }
            
            chromaAsset.setImageX(chromaAsset.getImageX() + (CANVAS_WIDTH / 2));
            chromaAsset.setImageY(chromaAsset.getImageY() + (CANVAS_HEIGHT / 2));
        }
        
        if (chromaAsset.getImageName().contains("_sd_")) {
            chromaAsset.setShadow(true);
            chromaAsset.setZ(Integer.MIN_VALUE);
        } else {
            chromaAsset.setZ(chromaAsset.getZ() + chromaAsset.getLayer());
        }
    }

    private List<ChromaAsset> createBuildQueue() {
        if (renderState > maxStates) {
            renderState = 0;
        }
        
        List<ChromaAsset> candidates = assets.stream()
            .filter(x -> x.isSmall() == isSmallFurni)
            .collect(Collectors.toList());
        
        List<ChromaAsset> validDirections = candidates.stream()
            .filter(x -> x.getDirection() == renderDirection)
            .collect(Collectors.toList());
        
        if (validDirections.isEmpty()) {
            renderDirection = 0;
            validDirections = candidates.stream().filter(x -> x.getDirection() == renderDirection).collect(Collectors.toList());
        }
        if (validDirections.isEmpty()) {
            renderDirection = 2;
            validDirections = candidates.stream().filter(x -> x.getDirection() == renderDirection).collect(Collectors.toList());
        }
        if (validDirections.isEmpty()) {
            renderDirection = 4;
            validDirections = candidates.stream().filter(x -> x.getDirection() == renderDirection).collect(Collectors.toList());
        }
        if (validDirections.isEmpty()) {
            renderDirection = 6;
            validDirections = candidates.stream().filter(x -> x.getDirection() == renderDirection).collect(Collectors.toList());
        }
        
        candidates = validDirections;
        List<ChromaAsset> renderFrames = new ArrayList<>();
        
        for (int layer = 0; layer < this.highestAnimationLayer; layer++) {
            int frame = 0;
            
            if (animations.containsKey(layer) &&
                !animations.get(layer).getStates().isEmpty() &&
                animations.get(layer).getStates().containsKey(renderState)) {
                
                ChromaFrame frameData = animations.get(layer).getStates().get(renderState);
                if (!frameData.getFrames().isEmpty()) {
                    frame = Integer.parseInt(frameData.getFrames().get(0));
                }
            }
            
            final int frameToFind = frame;
            final int layerToFind = layer;
            renderFrames.addAll(candidates.stream()
                .filter(x -> x.getLayer() == layerToFind && x.getFrame() == frameToFind)
                .collect(Collectors.toList()));
        }
        
        if (!this.renderShadows) {
            renderFrames = renderFrames.stream()
                .filter(x -> !x.isShadow())
                .collect(Collectors.toList());
        }
        
        renderFrames.sort(Comparator.comparingInt(ChromaAsset::getZ));
        return renderFrames;
    }

    public byte[] createImage() {
        List<ChromaAsset> buildQueue = createBuildQueue();
        
        if (buildQueue == null || buildQueue.isEmpty()) {
            return null;
        }
        
        List<Color> cropColours = new ArrayList<>();
        
        if (this.cropImage) {
            if (this.renderBackground) {
                cropColours.add(new Color(142, 142, 94));
                cropColours.add(new Color(152, 152, 101));
            } else {
                cropColours.add(hexToColor(this.renderCanvasColour));
            }
        }
        
        BufferedImage canvas = copyImage(drawingCanvas);
        Graphics2D g = canvas.createGraphics();
        g.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
        g.setRenderingHint(RenderingHints.KEY_INTERPOLATION, RenderingHints.VALUE_INTERPOLATION_BILINEAR);
        
        for (ChromaAsset asset : buildQueue) {
            String imagePath = asset.getImagePath();
            if (imagePath == null) continue;
            
            try {
                BufferedImage image = ImageIO.read(new File(imagePath));
                
                if (asset.getAlpha() != -1) {
                    image = tintImage(image, "FFFFFF", asset.getAlpha());
                }
                
                if (asset.getColourCode() != null) {
                    image = tintImage(image, asset.getColourCode(), 255);
                }
                
                if (asset.isShadow()) {
                    image = applyOpacity(image, 0.2f);
                }
                
                // Handle different ink modes
                Composite originalComposite = g.getComposite();
                if ("ADD".equals(asset.getInk()) || "33".equals(asset.getInk())) {
                    // For ADD blending, we'll use a lighter composite
                    g.setComposite(AlphaComposite.getInstance(AlphaComposite.SRC_OVER));
                }
                
                int x = canvas.getWidth() - asset.getImageX();
                int y = canvas.getHeight() - asset.getImageY();
                g.drawImage(image, x, y, null);
                
                g.setComposite(originalComposite);
                
            } catch (IOException e) {
                System.err.println("Error loading image: " + imagePath);
                e.printStackTrace();
            }
        }
        
        g.dispose();
        
        BufferedImage finalImage = canvas;
        
        if (cropImage && !cropColours.isEmpty()) {
            finalImage = ImageUtil.trimBitmap(canvas, cropColours.toArray(new Color[0]));
        }
        
        return renderImage(finalImage);
    }

    private byte[] renderImage(BufferedImage image) {
        try {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            ImageIO.write(image, "PNG", baos);
            return baos.toByteArray();
        } catch (IOException e) {
            e.printStackTrace();
            return null;
        }
    }

    private BufferedImage tintImage(BufferedImage image, String colourCode, int alpha) {
        Color rgb = hexToColor(colourCode);
        BufferedImage tinted = new BufferedImage(image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_ARGB);
        
        for (int x = 0; x < image.getWidth(); x++) {
            for (int y = 0; y < image.getHeight(); y++) {
                int pixel = image.getRGB(x, y);
                Color current = new Color(pixel, true);
                
                if (current.getAlpha() > 0) {
                    int r = (rgb.getRed() * current.getRed()) / 255;
                    int g = (rgb.getGreen() * current.getGreen()) / 255;
                    int b = (rgb.getBlue() * current.getBlue()) / 255;
                    
                    Color newColor = new Color(r, g, b, alpha);
                    tinted.setRGB(x, y, newColor.getRGB());
                } else {
                    tinted.setRGB(x, y, pixel);
                }
            }
        }
        
        return tinted;
    }

    private BufferedImage applyOpacity(BufferedImage image, float opacity) {
        BufferedImage result = new BufferedImage(image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_ARGB);
        Graphics2D g = result.createGraphics();
        g.setComposite(AlphaComposite.getInstance(AlphaComposite.SRC_OVER, opacity));
        g.drawImage(image, 0, 0, null);
        g.dispose();
        return result;
    }

    private BufferedImage copyImage(BufferedImage source) {
        BufferedImage copy = new BufferedImage(source.getWidth(), source.getHeight(), BufferedImage.TYPE_INT_ARGB);
        Graphics2D g = copy.createGraphics();
        g.drawImage(source, 0, 0, null);
        g.dispose();
        return copy;
    }

    public static Color hexToColor(String hexString) {
        if ("transparent".equalsIgnoreCase(hexString)) {
            return new Color(0, 0, 0, 0);
        }
        
        try {
            hexString = hexString.replace("#", "");
            if (hexString.length() == 6) {
                int r = Integer.parseInt(hexString.substring(0, 2), 16);
                int g = Integer.parseInt(hexString.substring(2, 4), 16);
                int b = Integer.parseInt(hexString.substring(4, 6), 16);
                return new Color(r, g, b);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        
        return new Color(254, 254, 254);
    }

    public String getFileName() {
        String name = (isSmallFurni ? "s_" : "") + sprite + "_" + renderDirection + "_" + renderState;
        
        long coloredAssets = assets.stream().filter(x -> x.getColourCode() != null).count();
        if (this.colourId > -1 && coloredAssets > 0) {
            name += "_colour" + this.colourId;
        }
        
        return name + ".png";
    }

    private String getFileNameWithoutExtension(String fileName) {
        int lastDot = fileName.lastIndexOf('.');
        int lastSlash = Math.max(fileName.lastIndexOf('/'), fileName.lastIndexOf('\\'));
        
        if (lastDot > lastSlash) {
            return fileName.substring(lastSlash + 1, lastDot);
        }
        return fileName.substring(lastSlash + 1);
    }

    // Getters
    public String getOutputDirectory() {
        return Paths.get("furni_export", sprite).toString();
    }

    public String getXmlDirectory() {
        return Paths.get("furni_export", sprite, "xml").toString();
    }

    public int getMaxStates() { return maxStates; }
    public boolean isSmallFurni() { return isSmallFurni; }
    public int getRenderDirection() { return renderDirection; }
    public int getColourId() { return colourId; }
    public String getSprite() { return sprite; }
    public boolean isIcon() { return isIcon; }
    public int getRenderState() { return renderState; }

    // Helper class for NodeList
    private static class NodeListWrapper implements NodeList {
        private final List<Node> nodes;
        
        public NodeListWrapper(List<Node> nodes) {
            this.nodes = nodes;
        }
        
        @Override
        public Node item(int index) {
            return index >= 0 && index < nodes.size() ? nodes.get(index) : null;
        }
        
        @Override
        public int getLength() {
            return nodes.size();
        }
    }
}
