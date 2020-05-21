using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Bushtail_Sports.Utils
{
    class ImageToolbox
    {
        class LockedFastImage
        {
            private Bitmap image;
            private byte[] rgbValues;
            private BitmapData bmpData;

            private IntPtr ptr;
            private int bytes;

            public LockedFastImage(Bitmap image)
            {
                this.image = image;
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);

                ptr = bmpData.Scan0;
                bytes = Math.Abs(bmpData.Stride) * image.Height;
                rgbValues = new byte[bytes];
                Marshal.Copy(ptr, rgbValues, 0, bytes);
            }

            ~LockedFastImage()
            {
                try
                {
                    image.UnlockBits(bmpData);
                }
                catch { }
            }

            /// <summary>
            /// Returns or sets a pixel of the image. 
            /// </summary>
            /// <param name="x">x parameter of the pixel</param>
            /// <param name="y">y parameter of the pixel</param>
            public Color this[int x, int y]
            {
                get
                {
                    int index = (x + (y * image.Width)) * 4;
                    return Color.FromArgb(rgbValues[index + 3], rgbValues[index + 2], rgbValues[index + 1], rgbValues[index]);
                }

                set
                {
                    int index = (x + (y * image.Width)) * 4;
                    rgbValues[index] = value.B;
                    rgbValues[index + 1] = value.G;
                    rgbValues[index + 2] = value.R;
                    rgbValues[index + 3] = value.A;
                }
            }

            /// <summary>
            /// Width of the image. 
            /// </summary>
            public int Width
            { get => image.Width; }

            /// <summary>
            /// Height of the image. 
            /// </summary>
            public int Height
            { get => image.Height; }

            /// <summary>
            /// Returns the modified Bitmap. 
            /// </summary>
            public Bitmap asBitmap()
            {
                Marshal.Copy(rgbValues, 0, ptr, bytes);
                return image;
            }
        }

        class ImageChecker
        {

            private LockedFastImage big_image;
            private LockedFastImage small_image;
            /// <summary>
            /// The time needed for last operation.
            /// </summary>
            public TimeSpan time_needed = new TimeSpan();

            /// <summary>
            /// Error return value.
            /// </summary>
            static public Point CHECKFAILED = Point.Empty;

            /// <summary>
            /// Constructor of the ImageChecker
            /// </summary>
            /// <param name="big_image">The image containing the small image.</param>
            /// <param name="small_image">The image located in the big image.</param>
            public ImageChecker(Bitmap big_image, Bitmap small_image)
            {
                this.big_image = new LockedFastImage(big_image);
                this.small_image = new LockedFastImage(small_image);
            }

            /// <summary>
            /// Returns the location of the small image in the big image. Returns CHECKFAILED if not found.
            /// </summary>
            /// <param name="x_speedUp">speeding up at x achsis.</param>
            /// <param name="y_speedUp">speeding up at y achsis.</param>
            /// <param name="begin_percent_x">Reduces the search rect. 0 - 100</param>
            /// <param name="end_percent_x">Reduces the search rect. 0 - 100</param>
            /// <param name="begin_percent_x">Reduces the search rect. 0 - 100</param>
            /// <param name="end_percent_y">Reduces the search rect. 0 - 100</param>
            public Point bigContainsSmall(int x_speedUp = 1, int y_speedUp = 1, int begin_percent_x = 0, int end_percent_x = 100, int begin_percent_y = 0, int end_percent_y = 100)
            {
                /*
                 * SPEEDUP PARAMETER
                 * It might be enough to check each second or third pixel in the small picture.
                 * However... In most cases it would be enough to check 4 pixels of the small image for diablo porposes.
                 * */

                /*
                 * BEGIN, END PARAMETER
                 * In most cases we know where the image is located, for this we have the begin and end paramenters.
                 * */

                DateTime begin = DateTime.Now;

                if (x_speedUp < 1) x_speedUp = 1;
                if (y_speedUp < 1) y_speedUp = 1;
                if (begin_percent_x < 0 || begin_percent_x > 100) begin_percent_x = 0;
                if (begin_percent_y < 0 || begin_percent_y > 100) begin_percent_y = 0;
                if (end_percent_x < 0 || end_percent_x > 100) end_percent_x = 100;
                if (end_percent_y < 0 || end_percent_y > 100) end_percent_y = 100;

                int x_start = (int)((double)big_image.Width * ((double)begin_percent_x / 100.0));
                int x_end = (int)((double)big_image.Width * ((double)end_percent_x / 100.0));
                int y_start = (int)((double)big_image.Height * ((double)begin_percent_y / 100.0));
                int y_end = (int)((double)big_image.Height * ((double)end_percent_y / 100.0));

                /*
                 * We cant speed up the big picture, because then we have to check pixels in the small picture equal to the speeded up size 
                 * for each pixel in the big picture.
                 * Would give no speed improvement.
                 * */

                //+ 1 because first pixel is in picture. - small because image have to be fully in the other image
                for (int x = x_start; x < x_end - small_image.Width + 1; x++)
                    for (int y = y_start; y < y_end - small_image.Height + 1; y++)
                    {
                        //now we check if all pixels matches
                        for (int sx = 0; sx < small_image.Width; sx += x_speedUp)
                            for (int sy = 0; sy < small_image.Height; sy += y_speedUp)
                            {
                                if (small_image[sx, sy] != big_image[x + sx, y + sy])
                                    goto CheckFailed;
                            }

                        //check ok
                        time_needed = DateTime.Now - begin;
                        return new Point(x, y);

                        CheckFailed:;
                    }

                time_needed = DateTime.Now - begin;
                return CHECKFAILED;
            }
        }
    }

    class SearchImage
    {

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);


        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static Bitmap CaptureWindow(IntPtr hwnd, int Width, int Height)
        {
            Bitmap screenBmp = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(screenBmp);

            IntPtr dc1 = GetDC(hwnd);
            IntPtr dc2 = g.GetHdc();

            //Main drawing, copies the screen to the bitmap
            //last number is the copy constant
            BitBlt(dc2, 0, 0, Width, Height, dc1, 0, 0, 13369376);

            //Clean up
            ReleaseDC(hwnd, dc1);
            g.ReleaseHdc(dc2);
            g.Dispose();

            return screenBmp;
        }

        public static Point Find(Bitmap haystack, Bitmap needle, out Point point)
        {
            point = Point.Empty;
            int Now = DateTime.Now.Millisecond;
            if (null == haystack || null == needle)
            {
                return Point.Empty;
            }
            if (haystack.Width < needle.Width || haystack.Height < needle.Height)
            {
                return Point.Empty;
            }

            var haystackArray = GetPixelArray(haystack);
            var needleArray = GetPixelArray(needle);

            foreach (var firstLineMatchPoint in FindMatch(haystackArray.Take(haystack.Height - needle.Height), needleArray[0]))
            {
                if (IsNeedlePresentAtLocation(haystackArray, needleArray, firstLineMatchPoint, 1))
                {
                    point = firstLineMatchPoint;
                    return firstLineMatchPoint;
                }
            }
            return Point.Empty;
        }

        public static Color GetPixel(Bitmap bitmap, int X, int Y)
        {
            LockBitmap bit = new LockBitmap(bitmap);
            bit.LockBits();
            Color color = bit.GetPixel(X, Y);
            bit.UnlockBits();
            return color;
        }

        public static Point PixelSearch(Bitmap image, Color color)
        {
            List<Point> result = new List<Point>();
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (color.Equals(image.GetPixel(x, y)))
                    {
                        return new Point(x, y);
                    }
                }
            }

            return Point.Empty;
        }

        public static Bitmap GetBitmapArea(Bitmap bitmap, int fX, int fY, int sX, int sY)
        {
            /*
            Draw a Rectangle from two X,Y Coordinations 
            2th pos - 1th pos == Size of Rectangle
            */

            Rectangle r = new Rectangle(fX, fY, (sX - fX), (sY - fY));
            Bitmap nb = new Bitmap(r.Width, r.Height);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(bitmap, -r.X, -r.Y);
            return nb;
        }

        private static int[][] GetPixelArray(Bitmap bitmap)
        {
            var result = new int[bitmap.Height][];
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            for (int y = 0; y < bitmap.Height; ++y)
            {
                result[y] = new int[bitmap.Width];
                Marshal.Copy(bitmapData.Scan0 + y * bitmapData.Stride, result[y], 0, result[y].Length);
            }

            bitmap.UnlockBits(bitmapData);

            return result;
        }

        private static IEnumerable<Point> FindMatch(IEnumerable<int[]> haystackLines, int[] needleLine)
        {
            var y = 0;
            foreach (var haystackLine in haystackLines)
            {
                for (int x = 0, n = haystackLine.Length - needleLine.Length; x < n; ++x)
                {
                    if (ContainSameElements(haystackLine, x, needleLine, 0, needleLine.Length))
                    {
                        yield return new Point(x, y);
                    }
                }
                y += 1;
            }
        }

        private static bool ContainSameElements(int[] first, int firstStart, int[] second, int secondStart, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                if (first[i + firstStart] != second[i + secondStart])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsNeedlePresentAtLocation(int[][] haystack, int[][] needle, Point point, int alreadyVerified)
        {
            //we already know that "alreadyVerified" lines already match, so skip them
            for (int y = alreadyVerified; y < needle.Length; ++y)
            {
                if (!ContainSameElements(haystack[y + point.Y], point.X, needle[y], 0, needle[y].Length))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class LockBitmap
    {
        Bitmap source = null;
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;

        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public LockBitmap(Bitmap source)
        {
            this.source = source;
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            try
            {
                // Get width and height of bitmap
                Width = source.Width;
                Height = source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                             source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                Pixels = new byte[PixelCount * step];
                Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            try
            {
                // Copy data from byte array to pointer
                Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            Color clr = Color.Empty;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }
    }

}
