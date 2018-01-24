using System;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ColorPickerSample
{
    public sealed partial class MyColorPicker : UserControl
    {
        private byte[] bytes;
        private uint pixelWidth, pixelHeight;
        public event EventHandler ColorPickerCloseEvent;

        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.RegisterAttached(
            "SelectedBrush", typeof(SolidColorBrush), typeof(MyColorPicker), new PropertyMetadata(default(SolidColorBrush)));

        public SolidColorBrush SelectedBrush
        {
            get { return (SolidColorBrush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        public MyColorPicker()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            selectColorBorder.Background = SelectedBrush;
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

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedBrush = selectColorBorder.Background as SolidColorBrush;
            ColorPickerCloseEvent(this, new EventArgs());
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerCloseEvent(this, new EventArgs());
        }

        private void imgColorPicker_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(imgColorPicker);
            selectColorBorder.Background = new SolidColorBrush(GetPixel(bytes, (int)point.Position.X, (int)point.Position.Y, pixelWidth, pixelHeight));

        }
    }
}
