using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Services.Maps;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
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
        BitmapImage bg;

        public MainPage()
        {
            this.InitializeComponent();

            inkPres = drawingCanvas.InkPresenter;
            inkPres.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
            zoomingContainer.Width = this.ActualWidth;
            UpdatePenConfig();

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

        private async Task<BitmapImage> OpenPDF(StorageFile f)
        {
            try
            {
                PdfDocument doc = await PdfDocument.LoadFromFileAsync(f);

                PdfPage page = doc.GetPage(0);

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    bg = new BitmapImage();
                    bg.DecodePixelHeight = (int)drawingContainer.Height;
                    bg.DecodePixelType = DecodePixelType.Logical;
                    await page.RenderToStreamAsync(stream);
                    await bg.SetSourceAsync(stream);
                    pageBackground.Source = bg;
                    return bg;
                }
            } catch
            {
                return null;
            }

        }

        private async void ImportFileClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            importFileBtn.IsChecked = false;
            FileOpenPicker picker = new FileOpenPicker();

            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".pdf");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile f = await picker.PickSingleFileAsync();
            if (f == null)
                return;

            if (f.FileType.Equals(".pdf"))
                await OpenPDF(f);
        }

        private async void SaveFileClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            saveFile.IsChecked = false;
            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeChoices = { { "Ink File", new string[] { ".isf" } } },
                SuggestedFileName = "New Note",
                CommitButtonText = "Save Note",
            };


            StorageFile f = await picker.PickSaveFileAsync();

            if (f == null)
                return;

            MemoryStream saveStream = new MemoryStream();
            await inkPres.StrokeContainer.SaveAsync(saveStream.AsOutputStream());
            await FileIO.WriteBytesAsync(f, saveStream.ToArray());
        }

        private async void OpenFileClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            openFile.IsChecked = false;

            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".isf" }
            };

            StorageFile f = await picker.PickSingleFileAsync();

            Stream stream = await f.OpenStreamForReadAsync();
            await inkPres.StrokeContainer.LoadAsync(stream.AsInputStream());
        }
    }

    public class NotingPage
    {
        BitmapImage bg; // Background image
        ObservableCollection<InkStroke> strokes; // Ink strokes

        public NotingPage(ObservableCollection<InkStroke> strokes)
        {
            this.strokes = strokes;
        }

        public NotingPage(ObservableCollection<InkStroke> strokes, BitmapImage bg)
        {
            this.bg = bg;
            this.strokes = strokes;
        }
    }

    public class NotingPageCollection : ObservableCollection<NotingPage>
    {
        ObservableCollection<NotingPage> pages;

        public NotingPageCollection(ObservableCollection<NotingPage> pages)
        {
            this.pages = pages;
        }
    }
}
