package com.quackster.chroma.extractor;

import com.jpexs.decompiler.flash.SWF;
import com.jpexs.decompiler.flash.tags.DefineBinaryDataTag;
import com.jpexs.decompiler.flash.tags.Tag;
import com.jpexs.decompiler.flash.tags.base.ImageTag;
import com.jpexs.decompiler.flash.types.RECT;
import com.quackster.chroma.util.FileUtil;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import javax.imageio.ImageIO;
import java.awt.*;
import java.awt.geom.AffineTransform;
import java.awt.image.AffineTransformOp;
import java.awt.image.BufferedImage;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.HashMap;
import java.util.Map;

/**
 * Extracts furniture data from SWF files
 * Uses JPEXS Free Flash Decompiler library for SWF parsing
 */
public class FurniExtractor {
    
    /**
     * Parses a SWF file and extracts furniture assets and metadata
     * @param file Path to the SWF file
     * @return true if parsing was successful
     */
    public static boolean parse(String file) {
        try {
            File swfFile = new File(file);
            if (!swfFile.exists()) {
                System.err.println("SWF file not found: " + file);
                return false;
            }
            
            String fileName = getFileNameWithoutExtension(file);
            
            // Create output directories
            Path exportDir = Paths.get("furni_export", fileName);
            Path xmlDir = Paths.get("furni_export", fileName, "xml");
            
            Files.createDirectories(exportDir);
            Files.createDirectories(xmlDir);
            
            // Parse SWF using JPEXS library
            FileInputStream fis = new FileInputStream(swfFile);
            SWF swf = new SWF(fis, false);
            fis.close();
            
            Map<Integer, String> symbolClass = new HashMap<>();
            Map<Integer, ImageTag> imageTags = new HashMap<>();
            
            // Extract symbol class and tags
            for (Tag tag : swf.getTags()) {
                // Extract SymbolClass information
                if (tag instanceof com.jpexs.decompiler.flash.tags.SymbolClassTag) {
                    com.jpexs.decompiler.flash.tags.SymbolClassTag symbolClassTag = 
                        (com.jpexs.decompiler.flash.tags.SymbolClassTag) tag;
                    
                    for (int i = 0; i < symbolClassTag.tags.size(); i++) {
                        symbolClass.put(symbolClassTag.tags.get(i), symbolClassTag.names.get(i));
                    }
                }
                
                // Extract binary data (XML files)
                if (tag instanceof DefineBinaryDataTag) {
                    DefineBinaryDataTag dataTag = (DefineBinaryDataTag) tag;
                    
                    if (symbolClass.containsKey(dataTag.getCharacterId())) {
                        String name = symbolClass.get(dataTag.getCharacterId());
                        String[] parts = name.split("_");
                        String type = parts[parts.length - 1];
                        
                        byte[] data = dataTag.binaryData.getRangeData();
                        String txt = new String(data, StandardCharsets.UTF_8);
                        
                        Path xmlFile = xmlDir.resolve(type + ".xml");
                        if (!Files.exists(xmlFile)) {
                            Files.writeString(xmlFile, txt, StandardCharsets.UTF_8);
                        }
                    }
                }
                
                // Extract images
                if (tag instanceof ImageTag) {
                    ImageTag imageTag = (ImageTag) tag;
                    imageTags.put(imageTag.getCharacterId(), imageTag);
                }
            }
            
            // Write images for each symbol
            for (Map.Entry<Integer, String> entry : symbolClass.entrySet()) {
                int symbolId = entry.getKey();
                String symbolName = entry.getValue();
                
                if (!imageTags.containsKey(symbolId)) {
                    continue;
                }
                
                ImageTag imageTag = imageTags.get(symbolId);
                String xmlName = symbolName.substring(fileName.length() + 1);
                Path imagePath = exportDir.resolve(xmlName + ".png");
                
                if (!Files.exists(imagePath)) {
                    try {
                        BufferedImage image = imageTag.getImage();
                        ImageIO.write(image, "PNG", imagePath.toFile());
                    } catch (Exception e) {
                        System.err.println("Error extracting image " + xmlName + ": " + e.getMessage());
                    }
                }
            }
            
            // Process assets and handle flipH
            Document assetDocument = FileUtil.solveXmlFile(xmlDir.toString(), "assets");
            if (assetDocument != null) {
                NodeList assets = assetDocument.getElementsByTagName("asset");
                
                for (int i = 0; i < assets.getLength(); i++) {
                    Node asset = assets.item(i);
                    
                    Node sourceAttr = asset.getAttributes().getNamedItem("source");
                    if (sourceAttr == null) {
                        continue;
                    }
                    
                    String source = sourceAttr.getNodeValue();
                    Node nameAttr = asset.getAttributes().getNamedItem("name");
                    if (nameAttr == null) {
                        continue;
                    }
                    
                    String imageName = nameAttr.getNodeValue();
                    
                    String assetImage = FileUtil.solveFile(exportDir.toString(), source);
                    String newName = imageName + ".png";
                    Path newPath = exportDir.resolve(newName);
                    
                    if (assetImage != null && !Files.exists(newPath)) {
                        Files.copy(Paths.get(assetImage), newPath);
                        
                        // Handle horizontal flip
                        Node flipHAttr = asset.getAttributes().getNamedItem("flipH");
                        if (flipHAttr != null && "1".equals(flipHAttr.getNodeValue())) {
                            BufferedImage bitmap = ImageIO.read(newPath.toFile());
                            
                            // Flip horizontally
                            AffineTransform tx = AffineTransform.getScaleInstance(-1, 1);
                            tx.translate(-bitmap.getWidth(), 0);
                            AffineTransformOp op = new AffineTransformOp(tx, AffineTransformOp.TYPE_NEAREST_NEIGHBOR);
                            bitmap = op.filter(bitmap, null);
                            
                            // Rotate 180 degrees
                            AffineTransform rotate = AffineTransform.getRotateInstance(Math.PI, 
                                bitmap.getWidth() / 2.0, bitmap.getHeight() / 2.0);
                            AffineTransformOp rotateOp = new AffineTransformOp(rotate, AffineTransformOp.TYPE_BILINEAR);
                            bitmap = rotateOp.filter(bitmap, null);
                            
                            ImageIO.write(bitmap, "PNG", newPath.toFile());
                        }
                    }
                }
            }
            
            return true;
            
        } catch (Exception e) {
            System.err.println("Error parsing SWF file: " + file);
            e.printStackTrace();
            return false;
        }
    }
    
    private static String getFileNameWithoutExtension(String fileName) {
        int lastDot = fileName.lastIndexOf('.');
        int lastSlash = Math.max(fileName.lastIndexOf('/'), fileName.lastIndexOf('\\'));
        
        if (lastDot > lastSlash) {
            return fileName.substring(lastSlash + 1, lastDot);
        }
        return fileName.substring(lastSlash + 1);
    }
}
