using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace ColorPickerSample
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private byte[] bytes;
        private uint pixelWidth, pixelHeight;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri((imgColorPicker.Source as BitmapImage).UriSource);
            using (IRandomAccessStream stream = await random.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                pixelWidth = decoder.PixelWidth;
                pixelHeight = decoder.PixelHeight;
                var pixels = await decoder.GetPixelDataAsync(
                                    BitmapPixelFormat.Rgba8,
                                    BitmapAlphaMode.Ignore,
                                    new BitmapTransform { ScaledHeight = pixelHeight, ScaledWidth = pixelWidth },
                                    ExifOrientationMode.IgnoreExifOrientation,
                                    ColorManagementMode.DoNotColorManage);
                bytes = pixels.DetachPixelData();
            }
        }

        public Color GetPixel(byte[] pixels, int x, int y, uint width, uint height)
        {
            int i = x;
            int j = y;
            int k = (j * (int)width + i) * 4;
            var r = pixels[k + 0];
            var g = pixels[k + 1];
            var b = pixels[k + 2];
            return Color.FromArgb(255, r, g, b);
        }



        private void txtColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string name = tb.Name;
            string text = tb.Text;
            try
            {
                Color color = (selectColorBorder.Background as SolidColorBrush).Color;
                byte newValue = 0;
                switch (name)
                {
                    case "txtColorR":
                        newValue = byte.Parse(tb.Text);
                        selectColorBorder.Background = new SolidColorBrush(Color.FromArgb(color.A, newValue, color.G, color.B));
                        break;
                    case "txtColorG":
                        newValue = byte.Parse(tb.Text);
                        selectColorBorder.Background = new SolidColorBrush(Color.FromArgb(color.A, color.R, newValue, color.B));
                        break;
                    case "txtColorB":
                        newValue = byte.Parse(tb.Text);
                        selectColorBorder.Background = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, newValue));
                        break;
                    case "txtColorHex":
                        selectColorBorder.Background = new SolidColorBrush(ToColor(tb.Text));
                        break;
                }
            }
            catch (Exception ex)
            {
                if (name.Equals("txtColorHex"))
                    tb.Text = "#FF000000";
                else
                    tb.Text = "00";
            }
        }

        public Color ToColor(string colorName)
        {
            if (colorName.StartsWith("#"))
                colorName = colorName.Replace("#", string.Empty);
            else
                return Colors.Black;

            int v = int.Parse(colorName, System.Globalization.NumberStyles.HexNumber);
            return new Color()
            {
                A = Convert.ToByte((v >> 24) & 255),
                R = Convert.ToByte((v >> 16) & 255),
                G = Convert.ToByte((v >> 8) & 255),
                B = Convert.ToByte((v >> 0) & 255)
            };
        }

        private void imgColorPicker_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(imgColorPicker);
            Debug.WriteLine(point.Position.X + " " + point.Position.Y);
            selectColorBorder.Background = new SolidColorBrush(GetPixel(bytes, (int)point.Position.X, (int)point.Position.Y, pixelWidth, pixelHeight));

        }
    }
}
