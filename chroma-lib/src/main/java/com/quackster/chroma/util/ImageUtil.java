package com.quackster.chroma.util;

import java.awt.*;
import java.awt.image.BufferedImage;
import java.util.function.IntPredicate;

/**
 * Utility class for image manipulation operations
 */
public class ImageUtil {
    
    /**
     * Trims transparent pixels from a bitmap
     * @param bmp The bitmap to trim
     * @return A trimmed bitmap
     */
    public static BufferedImage trimBitmap(BufferedImage bmp) {
        int w = bmp.getWidth();
        int h = bmp.getHeight();
        
        IntPredicate allTransparentRow = r -> {
            for (int i = 0; i < w; i++) {
                int alpha = (bmp.getRGB(i, r) >> 24) & 0xff;
                if (alpha != 0) {
                    return false;
                }
            }
            return true;
        };
        
        IntPredicate allTransparentColumn = c -> {
            for (int i = 0; i < h; i++) {
                int alpha = (bmp.getRGB(c, i) >> 24) & 0xff;
                if (alpha != 0) {
                    return false;
                }
            }
            return true;
        };
        
        int topmost = 0;
        for (int row = 0; row < h; row++) {
            if (!allTransparentRow.test(row)) {
                break;
            }
            topmost = row;
        }
        
        int bottommost = 0;
        for (int row = h - 1; row >= 0; row--) {
            if (!allTransparentRow.test(row)) {
                break;
            }
            bottommost = row;
        }
        
        int leftmost = 0;
        for (int col = 0; col < w; col++) {
            if (!allTransparentColumn.test(col)) {
                break;
            }
            leftmost = col;
        }
        
        int rightmost = 0;
        for (int col = w - 1; col >= 0; col--) {
            if (!allTransparentColumn.test(col)) {
                break;
            }
            rightmost = col;
        }
        
        if (rightmost == 0) rightmost = w;
        if (bottommost == 0) bottommost = h;
        
        int croppedWidth = rightmost - leftmost;
        int croppedHeight = bottommost - topmost;
        
        if (croppedWidth == 0) {
            leftmost = 0;
            croppedWidth = w;
        }
        
        if (croppedHeight == 0) {
            topmost = 0;
            croppedHeight = h;
        }
        
        try {
            BufferedImage target = new BufferedImage(croppedWidth, croppedHeight, BufferedImage.TYPE_INT_ARGB);
            Graphics2D g = target.createGraphics();
            g.drawImage(bmp, 0, 0, croppedWidth, croppedHeight,
                       leftmost, topmost, rightmost, bottommost, null);
            g.dispose();
            return target;
        } catch (Exception ex) {
            throw new RuntimeException(
                String.format("Values are topmost=%d btm=%d left=%d right=%d croppedWidth=%d croppedHeight=%d",
                    topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight), ex);
        }
    }
    
    /**
     * Trims pixels matching specific colors from a bitmap
     * @param bmp The bitmap to trim
     * @param colors Array of colors to trim
     * @return A trimmed bitmap
     */
    public static BufferedImage trimBitmap(BufferedImage bmp, Color... colors) {
        int w = bmp.getWidth();
        int h = bmp.getHeight();
        
        int topmost = 0;
        for (int row = 0; row < h; row++) {
            if (!allMatchingRow(bmp, row, colors)) {
                break;
            }
            topmost = row;
        }
        
        int bottommost = 0;
        for (int row = h - 1; row >= 0; row--) {
            if (!allMatchingRow(bmp, row, colors)) {
                break;
            }
            bottommost = row;
        }
        
        int leftmost = 0;
        for (int col = 0; col < w; col++) {
            if (!allMatchingCol(bmp, col, colors)) {
                break;
            }
            leftmost = col;
        }
        
        int rightmost = 0;
        for (int col = w - 1; col >= 0; col--) {
            if (!allMatchingCol(bmp, col, colors)) {
                break;
            }
            rightmost = col;
        }
        
        if (rightmost == 0) rightmost = w;
        if (bottommost == 0) bottommost = h;
        
        int croppedWidth = rightmost - leftmost;
        int croppedHeight = bottommost - topmost;
        
        if (croppedWidth == 0) {
            leftmost = 0;
            croppedWidth = w;
        }
        
        if (croppedHeight == 0) {
            topmost = 0;
            croppedHeight = h;
        }
        
        try {
            BufferedImage target = new BufferedImage(croppedWidth, croppedHeight, BufferedImage.TYPE_INT_ARGB);
            Graphics2D g = target.createGraphics();
            g.drawImage(bmp, 0, 0, croppedWidth, croppedHeight,
                       leftmost, topmost, rightmost, bottommost, null);
            g.dispose();
            return target;
        } catch (Exception ex) {
            throw new RuntimeException(
                String.format("Values are topmost=%d btm=%d left=%d right=%d croppedWidth=%d croppedHeight=%d",
                    topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight), ex);
        }
    }
    
    private static boolean allMatchingRow(BufferedImage bmp, int r, Color... colors) {
        int w = bmp.getWidth();
        
        for (int i = 0; i < w; i++) {
            Color pixel = new Color(bmp.getRGB(i, r), true);
            if (!colorEquals(colors, pixel)) {
                return false;
            }
        }
        
        return true;
    }
    
    private static boolean allMatchingCol(BufferedImage bmp, int c, Color... colors) {
        int h = bmp.getHeight();
        
        for (int i = 0; i < h; i++) {
            Color pixel = new Color(bmp.getRGB(c, i), true);
            if (!colorEquals(colors, pixel)) {
                return false;
            }
        }
        
        return true;
    }
    
    private static boolean colorEquals(Color[] colors, Color pixel) {
        for (Color color : colors) {
            if (pixel.getRed() == color.getRed() &&
                pixel.getGreen() == color.getGreen() &&
                pixel.getBlue() == color.getBlue() &&
                pixel.getAlpha() == color.getAlpha()) {
                return true;
            }
        }
        return false;
    }
}
