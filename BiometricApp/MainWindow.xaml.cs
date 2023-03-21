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

            MainChart.Plot.XLabel("Poziom");
            MainChart.Plot.YLabel("Częstość");


        }


        private void SaveImage(object sender, RoutedEventArgs e)
        {
         
            if (MainImage.Source != null)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)MainImage.Source));
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }

        }
        private void ShowBinaryMenu_Click(object sender, RoutedEventArgs e)
        {
            BinaryGrid.Visibility = Visibility.Visible;
            //foreach(var item in optionsGrid.Children)
            //{
            //    item
            //}
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
                        pixels[i] = (gray > thresholdValue) ? (byte)255 : (byte)0;
                        pixels[i + 1] = pixels[i + 2] = thresholdValue;
                    }

                    if (useGreenChannel)
                    {

                        pixels[i + 1] = (gray > thresholdValue) ? (byte)255 : (byte)0;
                        pixels[i + 2] = pixels[i] = thresholdValue;
                    }

                    if (useBlueChannel)
                    {
                        pixels[i + 2] = (gray > thresholdValue) ? (byte)255 : (byte)0;
                        pixels[i + 1] = pixels[i] = thresholdValue;
                    }
                    if (grayscaleChannel)
                    {
                        byte r = pixels[i + 0];
                        byte g = pixels[i + 1];
                        byte b = pixels[i + 2];

                        byte mean = (byte)((r + g + b) / 3);

                        pixels[i + 0] =
                        pixels[i + 1] =
                        pixels[i + 2] = mean > thresholdValue ? byte.MaxValue : byte.MinValue;
                    }


                    pixels[i + 3] = 255; // Alpha channel
                }


                BitmapSource bitmapSource2 = BitmapSource.Create(formatConvertedBitmap.PixelWidth, formatConvertedBitmap.PixelHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, formatConvertedBitmap.PixelWidth * 4);
                ProcessedImage.Source = bitmapSource2;
            }
        }


        public int[] ComputeHistogram(Bitmap image)
        {
            int[] histogram = new int[256];



            // Przejdź przez każdy piksel obrazu i zwiększ licznik odpowiadający jego wartości
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    System.Drawing.Color color = image.GetPixel(x, y);
                    int grayValue = (int)(( color.R +color.G + color.B))/3;
                    histogram[grayValue]++;
                }
            }

            return histogram;
        }



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

                    grayValue= (pixels[i] + pixels[i + 1] + pixels[i+2] )/3;
                    histogramData[grayValue]++;

                    //byte channelValue = 0;
                    //if (useRedChannel)
                    //{
                    //    channelValue = pixels[i];
                    //}

                    //if (useGreenChannel)
                    //{
                    //    channelValue = pixels[i + 1];
                    //}

                    //if (useBlueChannel)
                    //{
                    //    channelValue = pixels[i + 2];
                    //}

                    //if (grayscaleChannel)
                    //{
                    //    byte r = pixels[i + 0];
                    //    byte g = pixels[i + 1];
                    //    byte b = pixels[i + 2];

                    //    byte mean = (byte)((r + g + b) / 3);

                    //    channelValue = mean;
                    //}

                    //histogramData[channelValue]++;
                }
            #region
            //DisplayHistogram(histogramData);
            //StringBuilder histogramBuilder = new StringBuilder();
            //for (int i = 0; i < 256; i++)
            //{
            //    if (i % 20 == 0)
            //    {
            //        histogramBuilder.Append("\n");
            //    }
            //    histogramBuilder.AppendFormat("{0}: {1} ", i, histogramData[i]);
            //}

            //PlotModel histogramPlot = new PlotModel();

            ////create histogram series
            ////create histogram series
            //HistogramSeries histogramSeries = new HistogramSeries();
            ////histogramSeries.XAxisKey = histogramData;
            //histogramSeries.ItemsSource = histogramData;
            //histogramSeries.StrokeThickness = 1;
            //histogramSeries.StrokeColor = OxyColors.Blue;
            //histogramSeries.Title = "Histogram";

            //// create new window
            //Window histogramWindow = new Window();
            //histogramWindow.Title = "Histogram";
            //histogramWindow.Width = 800;
            //histogramWindow.Height = 600;

            //// create plot model and add histogram series
            //PlotModel plotModel = new PlotModel();
            //plotModel.Series.Add(histogramSeries);

            //// create plot view and set plot model
            //PlotView plotView = new PlotView();
            //plotView.Model = plotModel;

            //// add plot view to window content
            //histogramWindow.Content = plotView;
            //histogramWindow.ShowDialog();

            // show window
            #endregion


            double[] dataX = new double[256];
                for (int i = 0; i < 256; i++)
                {
                    dataX[i] = i;
                }

                
                
                MainChart.Plot.AddScatterStep(dataX, histogramData);
                
                MainChart.Refresh();

            }
            else
            {
                MessageBox.Show("Brak obrazu!");
            }
        }
        private void StreatchHistogram(object sender, RoutedEventArgs e)
        {
            Bitmap streachedBitmap = HistogramOperations.StretchingHistogram(bitmap);

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
            Bitmap streachedBitmap = HistogramOperations.HistogramEqualization(bitmap);

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
