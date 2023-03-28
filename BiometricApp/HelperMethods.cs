using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BiometricApp
{
    internal class HelperMethods
    {
        public static Bitmap BitmapImageToBitmap(BitmapImage bImg)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                // Create an encoder and save the image in the stream
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bImg));
                encoder.Save(outStream);

                // Create a new Bitmap object from the stream
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static BitmapSource CreateBitmapSource(Bitmap bmp)
        {
            using var memoryStream = new MemoryStream();
            bmp.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var bmpDecoder = BitmapDecoder.Create(
                memoryStream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);
            WriteableBitmap writeable = new WriteableBitmap(bmpDecoder.Frames.Single());
            writeable.Freeze();


            return writeable;
        }

        public static void DrawHistogram(WpfPlot plot, double[] histogramData)
        {
            plot.Plot.XLabel("Poziom");
            plot.Plot.YLabel("Częstość");

            double[] dataX = new double[256];
            for (int i = 0; i < 256; i++)
            {
                dataX[i] = i;
            }

            //MainChart.Plot.AddClevelandDot(dataX, histogramData);
            plot.Plot.AddLollipop(histogramData, dataX);

            plot.Refresh();
        }

    }

}
