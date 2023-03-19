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
        public static Bitmap StretchingHistogram(Bitmap image)
        {
            // Create an array to store the histogram of the image
            int[] histogram = new int[256];

            // Calculate the histogram of the image
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixel = image.GetPixel(x, y);
                    int grayValue = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                    histogram[grayValue]++;
                }
            }

            // Find the minimum and maximum gray value in the image
            int minGrayValue = 0;
            while (histogram[minGrayValue] == 0)
            {
                minGrayValue++;
            }
            int maxGrayValue = 255;
            while (histogram[maxGrayValue] == 0)
            {
                maxGrayValue--;
            }

            // Create a new bitmap to store the stretched image
            Bitmap stretchedImage = new Bitmap(image.Width, image.Height);

            // Stretch the image
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixel = image.GetPixel(x, y);
                    int grayValue = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                    int newGrayValue = (int)(((double)(grayValue - minGrayValue) / (maxGrayValue - minGrayValue)) * 255);
                    newGrayValue = Math.Max(0, Math.Min(255, newGrayValue));
                    System.Drawing.Color newPixel = System.Drawing.Color.FromArgb(newGrayValue, newGrayValue, newGrayValue);
                    stretchedImage.SetPixel(x, y, newPixel);
                }
            }

            return stretchedImage;
        }

        public unsafe static Bitmap HistogramEqualization(Bitmap image)
        {
            // konwertujemy bitmapę do obrazu 24-bitowego
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, image.PixelFormat);

            int pixelSize = Bitmap.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * pixelSize;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

            // tworzymy histogram
            int[] histogram = new int[256];
            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x = x + pixelSize)
                {
                    int pixelValue = currentLine[x];
                    histogram[pixelValue]++;
                }
            }

            // obliczamy dystrybuantę
            int[] cumulativeHistogram = new int[256];
            cumulativeHistogram[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cumulativeHistogram[i] = cumulativeHistogram[i - 1] + histogram[i];
            }

            // normalizujemy dystrybuantę
            for (int i = 0; i < 256; i++)
            {
                cumulativeHistogram[i] = (int)(((double)cumulativeHistogram[i] - cumulativeHistogram[0]) /
                    ((double)widthInBytes * heightInPixels - cumulativeHistogram[0]) * 255.0 + 0.5);
            }

            // wyrównujemy histogram
            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x = x + pixelSize)
                {
                    int pixelValue = currentLine[x];
                    currentLine[x] = (byte)cumulativeHistogram[pixelValue];
                }
            }

            // zwalniamy zasoby
            image.UnlockBits(bitmapData);

            return image;
        }

        public unsafe static Bitmap OtsuThreshold(Bitmap image)
        {
            // konwertujemy bitmapę do obrazu 24-bitowego
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, image.PixelFormat);

            int pixelSize = Bitmap.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * pixelSize;
            byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

            // tworzymy histogram
            int[] histogram = new int[256];
            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x = x + pixelSize)
                {
                    int pixelValue = currentLine[x];
                    histogram[pixelValue]++;
                }
            }

            // obliczamy całkowitą liczbę pikseli
            int totalPixels = widthInBytes * heightInPixels;

            // inicjujemy wartości
            double sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += i * histogram[i];
            }
            double sumB = 0;
            int wB = 0;
            int wF = 0;
            double varMax = 0;
            int threshold = 0;

            // iterujemy po wartościach pikseli
            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];
                if (wB == 0)
                {
                    continue;
                }

                wF = totalPixels - wB;
                if (wF == 0)
                {
                    break;
                }

                sumB += i * histogram[i];
                double meanB = sumB / wB;
                double meanF = (sum - sumB) / wF;

                double varBetween = (double)wB * (double)wF * (meanB - meanF) * (meanB - meanF);

                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = i;
                }
            }

            // binaryzujemy obraz
            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x = x + pixelSize)
                {
                    int pixelValue = currentLine[x];
                    byte newPixelValue = (byte)((pixelValue > threshold) ? 255 : 0);
                    currentLine[x] = newPixelValue;
                    currentLine[x + 1] = newPixelValue;
                    currentLine[x + 2] = newPixelValue;
                }
            }

            // zwalniamy zasoby
            image.UnlockBits(bitmapData);

            return image;
        }


    }
}
