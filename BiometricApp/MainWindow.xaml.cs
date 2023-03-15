using Microsoft.Win32;
using OxyPlot.Wpf;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace BiometricApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }


        public Bitmap Image { get; set; }
        public Bitmap ImageChanged { get; set; }
        //int[] histogram = null;

        private void OpenImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                OriginalImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                ProcessedImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
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
        }

        private void OnThresholdValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OriginalImage.Source != null)
            {
                BitmapImage bitmapImage = (BitmapImage)OriginalImage.Source;
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(bitmapImage, PixelFormats.Pbgra32, null, 0);

                byte thresholdValue = (byte)e.NewValue;
                byte[] pixels = new byte[formatConvertedBitmap.PixelWidth * formatConvertedBitmap.PixelHeight * 4];
                formatConvertedBitmap.CopyPixels(pixels, formatConvertedBitmap.PixelWidth * 4, 0);

                bool useRedChannel = RedChannelRadioButton.IsChecked ?? false;
                bool useGreenChannel = GreenChannelRadioButton.IsChecked ?? false;
                bool useBlueChannel = BlueChannelRadioButton.IsChecked ?? false;
                bool meanBlueChannel = MeanChannelRadioButton.IsChecked ?? false;

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte gray = (byte)((pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3.0);

                    if (useRedChannel)
                    {
                        pixels[i] = (gray > thresholdValue) ? (byte)255 : (byte)0;
                       
                    }

                    if (useGreenChannel)
                    {
                        pixels[i + 1] = (gray > thresholdValue) ? (byte)255 : (byte)0;
                    }

                    if (useBlueChannel)
                    {
                        pixels[i + 2] = (gray > thresholdValue) ? (byte)255 : (byte)0;
                    }
                    if (meanBlueChannel)
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

                BitmapSource bitmapSource = BitmapSource.Create(formatConvertedBitmap.PixelWidth, formatConvertedBitmap.PixelHeight, 96, 96, PixelFormats.Pbgra32, null, pixels, formatConvertedBitmap.PixelWidth * 4);
                ProcessedImage.Source = bitmapSource;
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
            if (OriginalImage.Source != null)
            {
                BitmapImage bitmapImage = (BitmapImage)OriginalImage.Source;
                FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(bitmapImage, PixelFormats.Pbgra32, null, 0);

                byte[] pixels = new byte[formatConvertedBitmap.PixelWidth * formatConvertedBitmap.PixelHeight * 4];
                formatConvertedBitmap.CopyPixels(pixels, formatConvertedBitmap.PixelWidth * 4, 0);

                bool useRedChannel = RedChannelRadioButton.IsChecked ?? false;
                bool useGreenChannel = GreenChannelRadioButton.IsChecked ?? false;
                bool useBlueChannel = BlueChannelRadioButton.IsChecked ?? false;
                bool meanBlueChannel = MeanChannelRadioButton.IsChecked ?? false;

                int[] histogramData = new int[256];
                int channelHistogram;
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte channelValue = 0;

                    if (useRedChannel)
                    {
                        channelValue = pixels[i];                    
                    }

                    if (useGreenChannel)
                    {
                        channelValue = pixels[i + 1];
                        
                    }

                    if (useBlueChannel)
                    {
                        channelValue = pixels[i + 2];
                        
                    }

                    if (meanBlueChannel)
                    {
                        byte r = pixels[i + 0];
                        byte g = pixels[i + 1];
                        byte b = pixels[i + 2];

                        byte mean = (byte)((r + g + b) / 3);

                        channelValue = mean;
                       
                    }

                    histogramData[channelValue]++;
                }

                //DisplayHistogram(histogramData);
                StringBuilder histogramBuilder = new StringBuilder();
                for (int i = 0; i < 256; i++)
                {
                    if (i % 20 == 0)
                    {
                        histogramBuilder.Append("\n");
                    }
                    histogramBuilder.AppendFormat("{0}: {1} ", i, histogramData[i]);
                }

                // show histogram in new window
                Window window = new Window();
                window.Content = histogramBuilder.ToString();
                window.ShowDialog();


                //PlotModel histogramPlot = new PlotModel();

                ////create histogram series
                ////create histogram series
                //HistogramSeries histogramSeries = new HistogramSeries();
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

                //// show window
                //histogramWindow.ShowDialog();

            }
        }
        public void DisplayHistogram(int[] data)
        {
            // Ustawienie wymiarów okna
            Window histogramWindow = new Window();
            histogramWindow.Width = 600;
            histogramWindow.Height = 400;

            // Utworzenie siatki w oknie
            UniformGrid grid = new UniformGrid();
            grid.Columns = data.Length;
            grid.Rows = 1;
            histogramWindow.Content = grid;

            // Wyznaczenie największej wartości w tablicy danych
            int maxData = data.Max();

            // Wypełnienie siatki prostokątami
            for (int i = 0; i < data.Length; i++)
            {
                var rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = System.Windows.Media.Brushes.Blue;
                rect.Width = 20;
                rect.Height = (data[i] * 300) / maxData;
                grid.Children.Add(rect);
            }

            // Wyświetlenie okna
            histogramWindow.ShowDialog();
        }


    }
}
