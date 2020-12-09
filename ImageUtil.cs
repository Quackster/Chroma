using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Drawing;
using System.IO;

namespace Chroma
{
    public class ImageUtil
    {
        public static Bitmap TrimBitmap(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            Func<int, bool> allWhiteRow = r =>
            {
                for (int i = 0; i < w; ++i)
                    if ((bmp.GetPixel(i, r).A != 0))
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = c =>
            {
                for (int i = 0; i < h; ++i)
                    if ((bmp.GetPixel(c, i).A != 0))
                        return false;
                return true;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (!allWhiteRow(row))
                    break;
                topmost = row;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (!allWhiteRow(row))
                    break;
                bottommost = row;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (!allWhiteColumn(col))
                    break;
                leftmost = col;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (!allWhiteColumn(col))
                    break;
                rightmost = col;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new System.Drawing.RectangleF(0, 0, croppedWidth, croppedHeight),
                      new System.Drawing.RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }

        public static Bitmap TrimBitmap(Bitmap bmp, params Rgba32[] colors)
        {
            int w = bmp.Width;
            int h = bmp.Height;


            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (!allWhiteRow(bmp, row, colors))
                    break;
                topmost = row;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (!allWhiteRow(bmp, row, colors))
                    break;
                bottommost = row;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (!allWhiteCol(bmp, col, colors))
                    break;
                leftmost = col;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (!allWhiteCol(bmp, col, colors))
                    break;
                rightmost = col;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new System.Drawing.RectangleF(0, 0, croppedWidth, croppedHeight),
                      new System.Drawing.RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }

        private static bool allWhiteRow(Bitmap bmp, int r, params Rgba32[] colors)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            for (int i = 0; i < w; ++i)
            {
                var pixel = bmp.GetPixel(i, r);

                if (!ColorEquals(colors, pixel)) 
                {
                    return false;
                }
            }

            return true;
        }

        private static bool allWhiteCol(Bitmap bmp, int c, params Rgba32[] colors)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            for (int i = 0; i < h; ++i)
            {
                var pixel = bmp.GetPixel(c, i);

                if (!ColorEquals(colors, pixel))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ColorEquals(Rgba32[] colors, System.Drawing.Color pixel)
        {
            foreach (var color in colors)
            {
                if (pixel.R == color.R && pixel.G == color.G && pixel.B == color.B && pixel.A == color.A)
                {
                    return true;
                }
            }

            return false;
        }

        public static unsafe void ColorToAlpha(Image<Rgba32> image, Rgba32 color)
        {
            double alpha1, alpha2, alpha3, alpha4;
            double* a1, a2, a3, a4;

            a1 = &alpha1;
            a2 = &alpha2;
            a3 = &alpha3;
            a4 = &alpha4;

            for (int j = 0; j < image.Height; j++)
            {
                var span = image.GetPixelRowSpan(j);

                for (int i = 0; i < span.Length; i++)
                {
                    ref Rgba32 pixel = ref span[i];

                    // Don't know what this is for
                    // *a4 = pixel.A;

                    if (pixel.R > color.R)
                        *a1 = (pixel.R - color.R) / (255.0 - color.R);
                    else if (pixel.R < color.R)
                        *a1 = (color.R - pixel.R) / color.R;
                    else
                        *a1 = 0.0;

                    if (pixel.G > color.G)
                        *a2 = (pixel.G - color.G) / (255.0 - color.G);
                    else if (pixel.G < color.G)
                        *a2 = (color.G - pixel.G) / color.G;
                    else
                        *a2 = 0.0;

                    if (pixel.B > color.B)
                        *a3 = (pixel.B - color.B) / (255.0 - color.B);
                    else if (pixel.B < color.B)
                        *a3 = (color.B - pixel.B) / color.B;
                    else
                        *a3 = 0.0;

                    if (*a1 > *a2)
                        *a4 = *a1 > *a3 ? *a1 * 255.0 : *a3 * 255.0;
                    else
                        *a4 = *a2 > *a3 ? *a2 * 255.0 : *a3 * 255.0;

                    if (*a4 < 1.0)
                        return;

                    pixel.R = (byte)Math.Truncate((255.0 * (*a1 - color.R) / *a4 + color.R));
                    pixel.G = (byte)Math.Truncate((255.0 * (*a2 - color.G) / *a4 + color.G));
                    pixel.B = (byte)Math.Truncate((255.0 * (*a3 - color.B) / *a4 + color.B));

                    pixel.A = (byte)Math.Truncate(*a4);
                }
            }
        }

        public static System.Drawing.Bitmap ToBitmap<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var memoryStream = new MemoryStream())
            {
                var imageEncoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);
                image.Save(memoryStream, imageEncoder);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return new System.Drawing.Bitmap(memoryStream);
            }
        }

        public static Image<TPixel> ToImageSharpImage<TPixel>(System.Drawing.Bitmap bitmap) where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return SixLabors.ImageSharp.Image.Load<TPixel>(memoryStream);
            }
        }
    }
}
