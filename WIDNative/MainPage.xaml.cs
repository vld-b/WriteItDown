using System;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WIDNative
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private InkPresenter inkPres;

        public MainPage()
        {
            this.InitializeComponent();

            inkPres = drawingCanvas.InkPresenter;
            inkPres.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
            zoomingContainer.Width = this.ActualWidth;
            UpdatePenConfig();

            this.Loaded += async (s, e) =>
            {
                await OpenPDF("C:/Users/vladb/Downloads/A4_hoch_kariert_rand.pdf");
            };

            this.SizeChanged += (s, e) =>
            {
                zoomingContainer.Width = e.NewSize.Width;
                zoomingContainer.Height = e.NewSize.Height;
                //float sizeChangedFactor = (float)((e.NewSize.Width*e.NewSize.Height) / (e.PreviousSize.Width*e.PreviousSize.Height));
                //drawingScrollView.MinZoomFactor = drawingScrollView.MinZoomFactor * sizeChangedFactor;
                //drawingScrollView.MaxZoomFactor *= sizeChangedFactor;
            };
        }

        private void UpdatePenConfig()
        {
            if (inkPres != null)
            {
                InkDrawingAttributes attributes = inkPres.CopyDefaultDrawingAttributes();

                attributes.FitToCurve = false;
                attributes.IgnorePressure = false;
                attributes.IgnoreTilt = false;

                inkPres.UpdateDefaultDrawingAttributes(attributes);
            }
        }

        private async Task<BitmapImage> OpenPDF(String path)
        {
            try
            {
                StorageFile f = await StorageFile.GetFileFromPathAsync(path);
                PdfDocument doc = await PdfDocument.LoadFromFileAsync(f);
            Console.WriteLine("Loaded PDF File");

            PdfPage page = doc.GetPage(0);
            Console.WriteLine("Loaded PDF Page");

            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                BitmapImage bg = new BitmapImage();
                Console.WriteLine("Created Bitmap");
                await page.RenderToStreamAsync(stream);
                await bg.SetSourceAsync(stream);
                Console.WriteLine("Configured BitMap");
                pageBackground.Source = bg;
                Console.WriteLine("Set Page Background");
                return bg;
            }
            } catch
            {
                Console.WriteLine("Failed to load PDF File");
                return null;
            }

        }
    }
}
