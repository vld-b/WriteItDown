using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            inkPres.InputDeviceTypes = CoreInputDeviceTypes.Pen;
            drawingContainer.Width = this.ActualWidth;
            UpdatePenConfig();
            this.SizeChanged += (s, e) =>
            {
                drawingContainer.Width = e.NewSize.Width;
                drawingContainer.Height = e.NewSize.Height-inkToolbar.ActualHeight;
            };
        }

        private void UpdatePenConfig()
        {
            if (inkPres != null)
            {
                InkDrawingAttributes attributes = inkPres.CopyDefaultDrawingAttributes();

                inkPres.UpdateDefaultDrawingAttributes(attributes);
            }
        }
    }
}
