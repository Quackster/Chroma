package com.quackster.chroma.util;

import org.w3c.dom.Document;
import org.xml.sax.SAXException;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

/**
 * Utility class for file operations, particularly XML file handling
 */
public class FileUtil {
    
    /**
     * Finds and parses an XML file in the given directory that contains the specified filename
     * @param outputDirectory The directory to search
     * @param fileNameContains The string that the filename should contain
     * @return Parsed XML Document or null if not found
     */
    public static Document solveXmlFile(String outputDirectory, String fileNameContains) {
        try {
            File dir = new File(outputDirectory);
            if (!dir.exists() || !dir.isDirectory()) {
                return null;
            }

            File[] files = dir.listFiles();
            if (files == null) {
                return null;
            }

            for (File file : files) {
                String fileNameWithoutExt = getFileNameWithoutExtension(file.getName());
                
                if (fileNameWithoutExt.contains(fileNameContains)) {
                    String text = Files.readString(file.toPath(), StandardCharsets.UTF_8);
                    
                    // Fix common XML issues
                    if (text.contains("\n<?xml")) {
                        text = text.replace("\n<?xml", "<?xml");
                        Files.writeString(file.toPath(), text, StandardCharsets.UTF_8);
                    }
                    
                    if (text.contains("<graphics>")) {
                        text = text.replace("<graphics>", "");
                        text = text.replace("</graphics>", "            ");
                        Files.writeString(file.toPath(), text, StandardCharsets.UTF_8);
                    }
                    
                    DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
                    DocumentBuilder builder = factory.newDocumentBuilder();
                    return builder.parse(file);
                }
            }
        } catch (IOException | ParserConfigurationException | SAXException e) {
            e.printStackTrace();
        }
        
        return null;
    }
    
    /**
     * Finds a file in the given directory that contains or ends with the specified filename
     * @param outputDirectory The directory to search
     * @param fileNameContains The string to match against filenames
     * @param endsWith If true, filename must end with the string; if false, just contain it
     * @return Full path to the file, or null if not found
     */
    public static String solveFile(String outputDirectory, String fileNameContains, boolean endsWith) {
        try {
            File dir = new File(outputDirectory);
            if (!dir.exists() || !dir.isDirectory()) {
                return null;
            }

            File[] files = dir.listFiles();
            if (files == null) {
                return null;
            }

            for (File file : files) {
                String fileNameWithoutExt = getFileNameWithoutExtension(file.getName());
                
                if (endsWith) {
                    if (fileNameWithoutExt.endsWith(fileNameContains)) {
                        return file.getAbsolutePath();
                    }
                } else {
                    if (fileNameWithoutExt.contains(fileNameContains)) {
                        return file.getAbsolutePath();
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        
        return null;
    }
    
    /**
     * Finds a file in the given directory that ends with the specified filename
     * @param outputDirectory The directory to search
     * @param fileNameContains The string that the filename should end with
     * @return Full path to the file, or null if not found
     */
    public static String solveFile(String outputDirectory, String fileNameContains) {
        return solveFile(outputDirectory, fileNameContains, true);
    }
    
    /**
     * Converts a numeric layer index to its corresponding letter
     * @param animationLayer The layer index
     * @return The corresponding lowercase letter
     */
    public static String numericLetter(int animationLayer) {
        if (animationLayer < 0 || animationLayer >= 26) {
            return "";
        }
        return String.valueOf((char) ('a' + animationLayer));
    }
    
    /**
     * Gets the filename without its extension
     * @param fileName The full filename
     * @return The filename without extension
     */
    private static String getFileNameWithoutExtension(String fileName) {
        int lastDotIndex = fileName.lastIndexOf('.');
        if (lastDotIndex > 0) {
            return fileName.substring(0, lastDotIndex);
        }
        return fileName;
    }
}
