using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiometricApp
{
    internal class HistogramOperations
    {
        public static Bitmap StretchingHistogram(Bitmap bmp, int[] red, int[] green, int[] blue)
        {
            //Tablice LUT dla skladowych
            int[] LUTred = calculateLUTforStretching(red);
            int[] LUTgreen = calculateLUTforStretching(green);
            int[] LUTblue = calculateLUTforStretching(blue);

            //Przetworz obraz i oblicz nowy histogram
            red = new int[256];
            green = new int[256];
            blue = new int[256];
            Bitmap oldBitmap = bmp;
            Bitmap newBitmap = new Bitmap(oldBitmap.Width, oldBitmap.Height, PixelFormat.Format24bppRgb);
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixel = oldBitmap.GetPixel(x, y);
                    Color newPixel = Color.FromArgb(LUTred[pixel.R], LUTgreen[pixel.G], LUTblue[pixel.B]);
                    newBitmap.SetPixel(x, y, newPixel);
                    red[newPixel.R]++;
                    green[newPixel.G]++;
                    blue[newPixel.B]++;
                }
            }
            return newBitmap;
        }

        private static int[] calculateLUTforStretching(int[] values)
        {
            //poszukaj wartości minimalnej
            int minValue = 0;
            for (int i = 0; i < 256; i++)
            {
                if (values[i] != 0)
                {
                    minValue = i;
                    break;
                }
            }
            //poszukaj wartości maksymalnej
            int maxValue = 255;
            for (int i = 255; i >= 0; i--)
            {
                if (values[i] != 0)
                {
                    maxValue = i;
                    break;
                }
            }
            //przygotuj tablice zgodnie ze wzorem
            int[] result = new int[256];
            double a = 255.0 / (maxValue - minValue);
            for (int i = 0; i < 256; i++)
            {
                result[i] = (int)(a * (i - minValue));
            }

            return result;
        }
        private static int[] calculateLUTforEqualization(int[] values, int size)
        {
            //poszukaj wartości minimalnej - czyli pierwszej niezerowej wartosci dystrybuanty
            double minValue = 0;
            for (int i = 0; i < 256; i++)
            {
                if (values[i] != 0)
                {
                    minValue = values[i];
                    break;
                }
            }

            //przygotuj tablice zgodnie ze wzorem
            int[] result = new int[256];
            double sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += values[i];
                result[i] = (int)(((sum - minValue) / (size - minValue)) * 255.0);
            }

            return result;
        }
        public unsafe static Bitmap HistogramEqualization(Bitmap bmp, int[] red, int[] green, int[] blue)
        {
            //Tablice LUT dla skladowych
            int[] LUTred = calculateLUTforEqualization(red, bmp.Width * bmp.Height);
            int[] LUTgreen = calculateLUTforEqualization(green, bmp.Width * bmp.Height);
            int[] LUTblue = calculateLUTforEqualization(blue, bmp.Width * bmp.Height);

            //Przetworz obraz i oblicz nowy histogram
            red = new int[256];
            green = new int[256];
            blue = new int[256];
            Bitmap oldBitmap = bmp;
            Bitmap newBitmap = new Bitmap(oldBitmap.Width, oldBitmap.Height, PixelFormat.Format24bppRgb);
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixel = oldBitmap.GetPixel(x, y);
                    Color newPixel = Color.FromArgb(LUTred[pixel.R], LUTgreen[pixel.G], LUTblue[pixel.B]);
                    newBitmap.SetPixel(x, y, newPixel);
                    red[newPixel.R]++;
                    green[newPixel.G]++;
                    blue[newPixel.B]++;
                }
            }
            //picture.Image = newBitmap;

            return newBitmap;
        }

        //public static Bitmap OtsuThreshold2(Bitmap bmp)
        //{


        //    return 
        //}

        public static Bitmap OtsuThreshold(Bitmap image)
        {
            // Konwertuj bitmapę na obraz w skali szarości
            Bitmap grayImage = ToGrayscale(image);

            // Zainicjuj histogram pikseli
            int[] histogram = Histogram(grayImage);

            // Oblicz rozmiar obrazu
            int totalPixels = grayImage.Width * grayImage.Height;

            // Oblicz wartość progową za pomocą algorytmu Otsu
            int threshold = ComputeThreshold(histogram, totalPixels);

            // Stwórz bitmapę wynikową
            Bitmap result = new Bitmap(grayImage.Width, grayImage.Height);

            // Przeprowadź binaryzację za pomocą wartości progowej
            for (int x = 0; x < grayImage.Width; x++)
            {
                for (int y = 0; y < grayImage.Height; y++)
                {
                    Color pixelColor = grayImage.GetPixel(x, y);
                    int grayValue = pixelColor.R;

                    // Jeśli wartość piksela jest mniejsza niż próg, ustaw na czarno, w przeciwnym przypadku ustaw na biało
                    if (grayValue < threshold)
                    {
                        result.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        result.SetPixel(x, y, Color.White);
                    }
                }
            }

            return result;
        }

        // Funkcja pomocnicza do konwersji bitmapy na obraz w skali szarości
        private static Bitmap ToGrayscale(Bitmap image)
        {
            Bitmap grayImage = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);

                    int grayValue = (int)((pixelColor.R * 0.3) + (pixelColor.G * 0.59) + (pixelColor.B * 0.11));

                    grayImage.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }

            return grayImage;
        }

        // Funkcja pomocnicza do obliczenia histogramu pikseli
        private static int[] Histogram(Bitmap image)
        {
            int[] histogram = new int[256];

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int grayValue = pixelColor.R;
                    histogram[grayValue]++;
                }
            }

            return histogram;
        }

        // Funkcja pomocnicza do obliczenia wartości progu algorytmem Otsu
        private static int ComputeThreshold(int[] histogram, int totalPixels)
        {
            double sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += i * histogram[i];
            }

            double sumB = 0;
            int wB = 0;
            int wF = 0;

            double maxVariance = 0;
            int threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];
                if (wB == 0) continue;

                wF = totalPixels - wB;
                if (wF == 0) break;

                sumB += i * histogram[i];
                double mB = sumB / wB;
                double mF = (sum - sumB) / wF;

                double betweenVariance = wB * wF * Math.Pow(mB - mF, 2);

                if (betweenVariance > maxVariance)
                {
                    maxVariance = betweenVariance;
                    threshold = i;
                }
            }

            return threshold;
        }

    }
}
