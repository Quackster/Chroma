package com.quackster.chromawebapp.controller;

import com.quackster.chroma.ChromaFurniture;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

/**
 * Main controller for furniture rendering requests
 */
@RestController
public class HomeController {
    
    private static final Logger logger = LoggerFactory.getLogger(HomeController.class);
    
    @GetMapping("/")
    public ResponseEntity<byte[]> index(
            @RequestParam(required = false) String sprite,
            @RequestParam(required = false, defaultValue = "false") String small,
            @RequestParam(required = false, defaultValue = "false") String s,
            @RequestParam(required = false, defaultValue = "0") String state,
            @RequestParam(required = false, defaultValue = "0") String direction,
            @RequestParam(required = false, defaultValue = "0") String rotation,
            @RequestParam(required = false, defaultValue = "0") String color,
            @RequestParam(required = false, defaultValue = "0") String colour,
            @RequestParam(required = false, defaultValue = "false") String bg,
            @RequestParam(required = false, defaultValue = "true") String crop,
            @RequestParam(required = false, defaultValue = "false") String shadow,
            @RequestParam(required = false, defaultValue = "transparent") String canvas,
            @RequestParam(required = false, defaultValue = "false") String icon) {
        
        try {
            // Parse parameters
            boolean isSmallFurni = parseBoolean(small) || parseBoolean(s);
            int renderState = parseNumeric(state, 0);
            int renderDirection = parseNumeric(direction, parseNumeric(rotation, 0));
            int colorId = Math.max(parseNumeric(color, 0), parseNumeric(colour, 0));
            boolean renderBackground = !("0".equals(bg) || "false".equalsIgnoreCase(bg));
            boolean renderShadows = "1".equals(shadow) || "true".equalsIgnoreCase(shadow);
            boolean cropImage = "1".equals(crop) || "true".equalsIgnoreCase(crop);
            String renderCanvasColour = canvas;
            boolean renderIcon = "1".equals(icon) || "true".equalsIgnoreCase(icon);
            
            // Validate state and color
            if (renderState >= 101) {
                renderState = 0;
            }
            if (colorId >= 16) {
                colorId = 0;
            }
            
            if (sprite == null || sprite.isEmpty()) {
                return ResponseEntity.status(HttpStatus.BAD_REQUEST).build();
            }
            
            // Create unique filename hash
            String fileNameUnique = sprite + isSmallFurni + renderState + renderDirection + 
                                  colorId + renderShadows + renderBackground + 
                                  renderCanvasColour + cropImage + renderIcon;
            String hashedUniqueName = hash(fileNameUnique);
            
            // Create export directory
            Path exportDir = Paths.get("furni_export", sprite, "export");
            Files.createDirectories(exportDir);
            
            Path cachedImagePath = exportDir.resolve(hashedUniqueName + ".png");
            
            // Check if cached image exists
            if (!Files.exists(cachedImagePath)) {
                logger.info("Generating furniture image for sprite: {}", sprite);
                
                String swfPath = "swfs/hof_furni/" + sprite + ".swf";
                File swfFile = new File(swfPath);
                
                if (!swfFile.exists()) {
                    logger.error("SWF file not found: {}", swfPath);
                    return ResponseEntity.status(HttpStatus.NOT_FOUND).build();
                }
                
                ChromaFurniture furni = new ChromaFurniture(
                    swfPath,
                    isSmallFurni,
                    renderState,
                    renderDirection,
                    colorId,
                    renderShadows,
                    renderBackground,
                    renderCanvasColour,
                    cropImage,
                    renderIcon
                );
                
                furni.run();
                byte[] bytes = furni.createImage();
                
                if (bytes != null && bytes.length > 0) {
                    Files.write(cachedImagePath, bytes);
                } else {
                    // Write empty file to cache the failure
                    Files.write(cachedImagePath, new byte[0]);
                    logger.warn("Generated empty image for sprite: {}", sprite);
                }
            }
            
            // Read and return the image
            if (Files.exists(cachedImagePath)) {
                byte[] imageBytes = Files.readAllBytes(cachedImagePath);
                
                if (imageBytes.length == 0) {
                    return ResponseEntity.status(HttpStatus.NOT_FOUND).build();
                }
                
                HttpHeaders headers = new HttpHeaders();
                headers.setContentType(MediaType.IMAGE_PNG);
                
                return ResponseEntity.ok()
                    .headers(headers)
                    .body(imageBytes);
            }
            
            return ResponseEntity.status(HttpStatus.NOT_FOUND).build();
            
        } catch (Exception e) {
            logger.error("Error generating furniture image", e);
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).build();
        }
    }
    
    /**
     * Parses a string parameter as a boolean
     */
    private boolean parseBoolean(String value) {
        return "1".equals(value) || "true".equalsIgnoreCase(value);
    }
    
    /**
     * Parses a numeric string parameter
     */
    private int parseNumeric(String value, int defaultValue) {
        if (value == null || value.isEmpty()) {
            return defaultValue;
        }
        
        try {
            return Integer.parseInt(value);
        } catch (NumberFormatException e) {
            return defaultValue;
        }
    }
    
    /**
     * Creates a SHA-1 hash of the input string
     */
    private static String hash(String input) {
        try {
            MessageDigest sha1 = MessageDigest.getInstance("SHA-1");
            byte[] hash = sha1.digest(input.getBytes());
            StringBuilder sb = new StringBuilder(hash.length * 2);
            
            for (byte b : hash) {
                sb.append(String.format("%02X", b));
            }
            
            return sb.toString();
        } catch (NoSuchAlgorithmException e) {
            throw new RuntimeException("SHA-1 algorithm not available", e);
        }
    }
}
