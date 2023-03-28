using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using OxyPlot.Series;

namespace BiometricApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public ImageSource defaultImage { get; set; }
        public Bitmap bitmap { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //BitmapImage bitmapImage = new BitmapImage(new Uri(@"D:\BiometricApp\BiometricApp\Resources\Lenna.png"));
            //BitmapImage bitmapImage = new BitmapImage(new Uri(@"D:\repos\BiometricApp\BiometricApp\Resources\Lenna.png"));
            //defaultImage = bitmapImage; // do resetu

            //bitmap =HelperMethods.BitmapImageToBitmap(bitmapImage);

            //MainImage.Source = bitmapImage;

 

        }


        private void SaveImage(object sender, RoutedEventArgs e)
        {
         
            if (ProcessedImage.Source != null)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)ProcessedImage.Source));
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
            else { MessageBox.Show("Brak obrazka do zapisania!"); }

        }
        private void ShowBinaryMenu_Click(object sender, RoutedEventArgs e)
        {
            BinaryGrid.Visibility = Visibility.Visible;
        }

        private void OpenImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*";
            BitmapImage bi;
            if (openFileDialog.ShowDialog() == true)
            {
                //bitmap= new Bitmap(openFileDialog.FileName);
                MainImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                bi= new BitmapImage(new Uri(openFileDialog.FileName));

                bitmap=HelperMethods.BitmapImageToBitmap(bi);
                defaultImage = MainImage.Source;
                ProcessedImage.Source=MainImage.Source;
           
            }
        }

        private void OnThresholdValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MainImage.Source != null)
            {
                BitmapSource bitmapSource = (BitmapSource)MainImage.Source;
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(bitmapSource, PixelFormats.Pbgra32, null, 0);

                byte thresholdValue = (byte)e.NewValue;
                byte[] pixels = new byte[formatConvertedBitmap.PixelWidth * formatConvertedBitmap.PixelHeight * 4];
                formatConvertedBitmap.CopyPixels(pixels, formatConvertedBitmap.PixelWidth * 4, 0);

                bool useRedChannel = RedChannelRadioButton.IsChecked ?? false;
                bool useGreenChannel = GreenChannelRadioButton.IsChecked ?? false;
                bool useBlueChannel = BlueChannelRadioButton.IsChecked ?? false;
                bool grayscaleChannel = GrayscaleChannelRadioButton.IsChecked ?? false;
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte gray = (byte)((pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3.0);

                    if (useRedChannel)
                    {
                        byte red = (gray > thresholdValue) ? (byte)255 : (byte)0;
                        pixels[i] = red;
                    }

                    if (useGreenChannel)
                    {
                        byte green = (gray > thresholdValue) ? (byte)255 : (byte)0;
                        pixels[i + 1] = green;
                    }

                    if (useBlueChannel)
                    {
                        byte blue = (gray > thresholdValue) ? (byte)255 : (byte)0;
                        pixels[i + 2] = blue;
                    }

                    if (grayscaleChannel)
                    {
                        byte mean = (byte)((pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3);

                        byte grayscale = (mean > thresholdValue) ? byte.MaxValue : byte.MinValue;

                        pixels[i] = pixels[i + 1] = pixels[i + 2] = grayscale;
                    }

                    pixels[i + 3] = 255; // Alpha channel
                }

                BitmapSource bitmapSource2 = BitmapSource.Create(formatConvertedBitmap.PixelWidth, formatConvertedBitmap.PixelHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, formatConvertedBitmap.PixelWidth * 4);
                ProcessedImage.Source = bitmapSource2;
            }
        }


        //public int[] ComputeHistogram(Bitmap image)
        //{
        //    int[] histogram = new int[256];

        //    // Przejdź przez każdy piksel obrazu i zwiększ licznik odpowiadający jego wartości
        //    for (int x = 0; x < image.Width; x++)
        //    {
        //        for (int y = 0; y < image.Height; y++)
        //        {
        //            System.Drawing.Color color = image.GetPixel(x, y);
        //            int grayValue = (int)(( color.R +color.G + color.B))/3;
        //            histogram[grayValue]++;
        //        }
        //    }

        //    return histogram;
        //}



        private void ShowHistogram(object sender, RoutedEventArgs e)
        {

            if (ProcessedImage.Source is BitmapSource bitmapImageSource)
            {
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(bitmapImageSource, PixelFormats.Pbgra32, null, 0);

                byte[] pixels = new byte[formatConvertedBitmap.PixelWidth * formatConvertedBitmap.PixelHeight * 4];
                formatConvertedBitmap.CopyPixels(pixels, formatConvertedBitmap.PixelWidth * 4, 0);

                bool useRedChannel = RedChannelRadioButton.IsChecked ?? false;
                bool useGreenChannel = GreenChannelRadioButton.IsChecked ?? false;
                bool useBlueChannel = BlueChannelRadioButton.IsChecked ?? false;
                bool grayscaleChannel = GrayscaleChannelRadioButton.IsChecked ?? false;

                double[] histogramData = new double[256];
                int grayValue;
                for (int i = 0; i < pixels.Length; i += 4)
                {


                    //grayValue= (pixels[i] + pixels[i + 1] + pixels[i+2] )/3;
                    //histogramData[grayValue]++;

                    byte channelValue = 0;
                    if (useRedChannel)
                    {
                        byte red = pixels[i];
                        histogramData[red]++;
                    }

                    if (useGreenChannel)
                    {
                        byte green = pixels[i+1];
                        histogramData[green]++;
                    }

                    if (useBlueChannel)
                    {
                        byte blue = pixels[i + 2];
                        histogramData[blue]++;
                    }

                    if (grayscaleChannel)
                    {
                        byte r = pixels[i + 0];
                        byte g = pixels[i + 1];
                        byte b = pixels[i + 2];

                        byte gray = (byte)((r + g + b) / 3.0);

                        histogramData[gray]++;
                    }

                }

                HelperMethods.DrawHistogram(MainChart, histogramData);

            }
            else
            {
                MessageBox.Show("Brak obrazu!");
            }
        }

        
        private void StreatchHistogram(object sender, RoutedEventArgs e)
        {

            //Oblicz histogram
           var red = new int[256];
           var green = new int[256];
           var blue = new int[256];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    System.Drawing.Color pixel = bitmap.GetPixel(x, y);
                    red[pixel.R]++;
                    green[pixel.G]++;
                    blue[pixel.B]++;
                }
            }


            Bitmap streachedBitmap = HistogramOperations.StretchingHistogram(bitmap,red,green,blue);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the bitmap to the memory stream
                streachedBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

                // Create a new BitmapImage and set its source to the memory stream
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                // Convert the BitmapImage to a BitmapSource
                ProcessedImage.Source = bitmapImage as BitmapSource;
            }
             
        }
        private void EqualizeHistogram(object sender, RoutedEventArgs e)
        {

          var red = new int[256];
          var green = new int[256];
          var blue = new int[256];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    System.Drawing.Color pixel = bitmap.GetPixel(x, y);
                    red[pixel.R]++;
                    green[pixel.G]++;
                    blue[pixel.B]++;
                }
            }


            Bitmap streachedBitmap = HistogramOperations.HistogramEqualization(bitmap,red,green,blue);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the bitmap to the memory stream
                streachedBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

                // Create a new BitmapImage and set its source to the memory stream
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                // Convert the BitmapImage to a BitmapSource
                ProcessedImage.Source = bitmapImage as BitmapSource;
            }

        }

        private void OtsuBinarization(object sender, RoutedEventArgs e)
        {
            Bitmap streachedBitmap = HistogramOperations.OtsuThreshold(bitmap);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the bitmap to the memory stream
                streachedBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);

                // Create a new BitmapImage and set its source to the memory stream
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                // Convert the BitmapImage to a BitmapSource
                ProcessedImage.Source = bitmapImage as BitmapSource;
            }

        }


        private void ResetImage(object sender, RoutedEventArgs e) => ProcessedImage.Source = defaultImage;




        private void ResetHistogram(object sender, RoutedEventArgs e)
        {
             MainChart.Reset();
            
        }


    }
}
