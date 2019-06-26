
using AutoPadInu_UWP.Model;
using OpenCVBridge;
using System;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Text;
using AutoPadInu_UWP.Model.Datatype;
using AutoPadInu_UWP.Model.ImageSource;
using AutoPadInu_UWP.Model.Tools;
using System.Diagnostics;
using System.Linq;
using AutoPadInu_UWP.Model.PanelTools;

namespace AutoPadInu_UWP
{
    public sealed partial class MainPage : Page
    {
        static readonly Size PanelImageSize = new Size(240, 200);

        FrameRenderer _SourceRenderer = null;
        FrameRenderer _ClippedRenderer = null;   
        FrameRenderer _ColorFilterRenderer = null;

        PuzzleSolver puzzleSolver=null;
        Parser PanelParser = null;

        //UI 
        Beads FilterSelectedBeadType = Beads.Notset;
        Point StartPoint, EndPoint;
        int TargetRouteScore = 600;

        Route ParsedRoute = null;
        SerialPort serialPort = null;

        StylusDriver stylusDriver =null;

        DispatcherTimer _FPSTimer = null;
        int _frameCount = 0;
        DispatcherTimer _MessageTimer = null;
        StringBuilder MessageSB=null;

        OpenCVHelper _cvhelper;
        Webcam webcam;

        BrightnessCalculator BrightnessCalc = null;

        //UI Settings
        bool AutoMode = false;
        bool FastMode = true;
        int Speed = 250;

        static readonly PhoneConfig phoneConfig = PhoneConfigs.iPhone6s;

        public MainPage()
        {
            this.InitializeComponent();

            puzzleSolver = new PuzzleSolver();
            PanelParser = new Parser();
            BrightnessCalc = new BrightnessCalculator();

            stylusDriver = new StylusDriver(phoneConfig);

            _SourceRenderer = new FrameRenderer(PreviewImage);
            _ColorFilterRenderer = new FrameRenderer(ColotFilterImage);
            _ClippedRenderer = new FrameRenderer(ClippedImage);

            _cvhelper = new OpenCVHelper();

            _FPSTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _FPSTimer.Tick += UpdateFPS;

            _MessageTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _MessageTimer.Tick += MessageTimer_Tick;
            MessageSB = new StringBuilder();

            webcam = new Webcam(100);
            webcam.OnReceiveFrame += Webcam_OnReceiveFrame;

            StartPoint = new Point(phoneConfig.PadPanelLocation.Left, phoneConfig.PadPanelLocation.Top);
            EndPoint = new Point(phoneConfig.PadPanelLocation.Right, phoneConfig.PadPanelLocation.Bottom);
            TargetScoreTextbox.Text = TargetRouteScore.ToString();

            FastModeCheckButton.IsChecked = FastMode;
            AutoModeCheckButton.IsChecked = AutoMode;
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            FilterCombobox.ItemsSource = Enum.GetValues(typeof(Beads));
            FilterCombobox.SelectedIndex = 0;

            SerialportCombobox.ItemsSource = await SerialPort.GetDeviceList();
            FilterCombobox.SelectedIndex = 0;

            StartPointX.Text = StartPoint.X.ToString();
            StartPointY.Text = StartPoint.Y.ToString();
            EndPointX.Text = EndPoint.X.ToString();
            EndPointY.Text = EndPoint.Y.ToString();
            SpeedTextBox.Text = Speed.ToString();

            await webcam.Initialize();
            _FPSTimer.Start();
            _MessageTimer.Start();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs args)
        {
            _FPSTimer.Stop();
            _MessageTimer.Stop();
            await webcam.Cleanup();
        }
        
        private void Webcam_OnReceiveFrame(SoftwareBitmap softwareBitmap)
        {
            SoftwareBitmap SourceBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            ProcessImage(SourceBitmap);
            Interlocked.Increment(ref _frameCount);
        }

        private async Task DoReset()
        {
            await Task.Factory.StartNew(async () => 
            {
                PanelParser.Clear();
                ParsedRoute = null;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
                {
                    ResultPanelTextBlock.Text = "";
                    RouteTextBox.Text = "";
                });
            });
        }

        private async Task DoParseRoute()
        {
            var ParsedPanel = PanelParser.GetParsedPanel();
            if (ParsedPanel!=null && ParsedPanel.IsValid())
            {
                MessageSB.AppendLine("Solving route.....");
                DateTime dt = DateTime.Now;
                
                PadPanel FinalPanel = null;

                await Task.Factory.StartNew(() =>
                {
                    ParsedRoute = PuzzleSolver.SolvePuzzleLoop(ParsedPanel, 40, TargetRouteScore);
                    FinalPanel = ParsedPanel.ApplyRoute(ParsedRoute);
                });

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    ResultPanelTextBlock.Text = FinalPanel.ToString();
                    RouteTextBox.Text = ParsedRoute.ToString();
                });

                int timems = (int)(DateTime.Now - dt).TotalMilliseconds;
                MessageSB.Append("Score:");
                MessageSB.Append(ParsedRoute.Score);
                MessageSB.Append(",len:");
                MessageSB.AppendLine(ParsedRoute.Length.ToString());
                MessageSB.Append("Finish in ");
                MessageSB.Append(timems.ToString());
                MessageSB.AppendLine(" ms");
            }
        }

        private async Task DoSendRoute()
        {
            if (serialPort != null && ParsedRoute != null)
            {
                await Task.Factory.StartNew(() =>
                {
                    MessageSB.AppendLine("Sending route...");
                    stylusDriver.SendRoute(ParsedRoute,FastMode,Speed).Wait();
                    MessageSB.AppendLine("Send route finish");
                });
            }
        }

        private async Task DoAutoMode()
        {
            await Task.Factory.StartNew(() =>
            {
                stylusDriver.AutoHome();
                while (AutoMode)
                {
                    DoReset().Wait();
                    PanelParser.Enable = true;
                    while (PanelParser.GetParsedPanel().IsValid() == false)
                        Task.Delay(100).Wait();
                    PanelParser.Enable = false;
                    MessageSB.AppendLine("Get panel");
                    DoParseRoute().Wait();
                    DoSendRoute().Wait();

                    Task.Delay(1200).Wait();
                    if (FastMode)
                    {
                        MessageSB.AppendLine("Waiting combo drop...");
                        BrightnessCalc.WaitUntilEdge(10000, BrightnessCalculator.Edges.Neg);
                    }

                    Task.Delay(1000).Wait();
                    MessageSB.AppendLine("Waiting next stage...");
                    BrightnessCalc.WaitUntilEdge(10000, BrightnessCalculator.Edges.Pos);

                    MessageSB.AppendLine("Next stage");
                }
                MessageSB.AppendLine("Auto Mode End");
            });
        }

        private async void ProcessImage(SoftwareBitmap SourceBitmap)
        {
            SoftwareBitmap RotatedSourceBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, SourceBitmap.PixelHeight, SourceBitmap.PixelWidth, BitmapAlphaMode.Premultiplied);
            _cvhelper.RotateImage(SourceBitmap, RotatedSourceBitmap, 90);
            
            int PanelWidth = (int)Math.Abs(EndPoint.X - StartPoint.X);
            int PanelHeight = (int)Math.Abs(EndPoint.Y - StartPoint.Y);

            SoftwareBitmap CroppedBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)PanelImageSize.Width, (int)PanelImageSize.Height, BitmapAlphaMode.Premultiplied);
            _cvhelper.CropImage(StartPoint, EndPoint, RotatedSourceBitmap, CroppedBitmap);
            BrightnessCalc.CalcBrightness(CroppedBitmap);
            
            SoftwareBitmap MaskedBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)PanelImageSize.Width, (int)PanelImageSize.Height, BitmapAlphaMode.Premultiplied);
            _cvhelper.MaskAndDrawPanelArea(CroppedBitmap, MaskedBitmap);

            using (var ImgResult = PanelParser.ParsePanelImage(MaskedBitmap))
            {
                var FilterImg = ImgResult?.SelectResult(FilterSelectedBeadType);
                if (FilterImg != null)
                    _ColorFilterRenderer.RenderFrame(SoftwareBitmap.Copy(FilterImg));
            }
            
            _SourceRenderer.RenderFrame(RotatedSourceBitmap);
            _ClippedRenderer.RenderFrame(CroppedBitmap);

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                LightThresTextbox.Text = "Thres:" + (BrightnessCalc.Threshold?.ToString("0.00") ?? String.Empty);
                LightValueTextbox.Text = "Value:" + (BrightnessCalc.Latest?.ToString("0.00") ?? String.Empty);
                AnalyzedTextBlock.Text = PanelParser.GetParsedPanel().ToString() ?? String.Empty;
            });
        }


        #region Timer Events
        private void UpdateFPS(object sender, object e)
        {
            var frameCount = Interlocked.Exchange(ref _frameCount, 0);
            this.FPSMonitor.Text = "FPS: " + frameCount;
        }

        private void MessageTimer_Tick(object sender, object e)
        {
            MessageTextBlock.Text = MessageSB.ToString();
            MessageTextboxScroll.ChangeView(null, MessageTextboxScroll.ScrollableHeight, null);
        }

        #endregion

        #region ButtonEvents

        private async void LoadImg_Click(object sender, RoutedEventArgs e)
        {
            webcam.Enable = false;

            var img = await ImageFileReader.SelectImage();
            if (img != null)
                ProcessImage(img);
        }

        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            BrightnessCalc.Clear();
            stylusDriver.AutoHome();
            await DoReset();
        }
        
        private async void ParseRoute_Click(object sender, RoutedEventArgs e)
        {
            PanelParser.Enable = true;
            while (PanelParser.GetParsedPanel().IsValid() == false)
                await Task.Delay(100);
            PanelParser.Enable = false;

            await DoParseRoute();
        }

        private async void SerialConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var node = SerialportCombobox.SelectedItem as DeviceNode;
                if (node != null)
                {
                    serialPort = await SerialPort.OpenByDeviceId(node.Id, 250000);
                    stylusDriver.SetSerialPort(serialPort);
                    MessageSB.AppendLine("Serial opened");
                }
                else
                {
                    MessageSB.AppendLine("Invalid Node");
                }
            }
            catch (Exception ex)
            {
                MessageSB.AppendLine(ex.Message);
                serialPort = null;
            }
        }

        private async void SendRoute_Click(object sender, RoutedEventArgs e)
        {
            await DoSendRoute();
        }

        private void ClearMessageButton_Click(object sender, RoutedEventArgs e)
        {
            lock (MessageSB)
                MessageSB.Clear();
        }

        private void Capture_Click(object sender, RoutedEventArgs e)
        {
            PanelParser.Enable = true;

            webcam.Enable = !webcam.Enable;
            MessageSB.Append("Webcam Enable ");
            MessageSB.AppendLine(webcam.Enable.ToString());
        }

        #endregion

        #region User Input
        private void Point_TextChanged(object sender, TextChangedEventArgs e)
        {
            StartPoint = new Point(int.TryParse(StartPointX.Text, out int x) ? x : 0, int.TryParse(StartPointY.Text, out int y) ? y : 0);
            EndPoint = new Point(int.TryParse(EndPointX.Text, out x) ? x : 0, int.TryParse(EndPointY.Text, out y) ? y : 0);
        }

        private void FilterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = sender as ComboBox;
            if (combobox?.SelectedItem is Beads)
                FilterSelectedBeadType = (Beads)combobox.SelectedItem;
            else
                FilterSelectedBeadType = Beads.Notset;
        }

        private void Speed_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(SpeedTextBox.Text, out int Value))
            {
                if (Value>100)
                    Speed = Value;
            }
        }
        
        private void ScoreSet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TargetScoreTextbox.Text, out int Value))
            {
                if (Value > 0)
                    TargetRouteScore = Value;
            }
        }

        private void FastModeCheckButton_CheckChanged(object sender, RoutedEventArgs e)
        {
            FastMode = FastModeCheckButton.IsChecked.Value;
            MessageSB.Append("Fast Mode ");
            MessageSB.AppendLine(FastMode.ToString());
        }
        
        private void AutoModeCheckButton_CheckChanged(object sender, RoutedEventArgs e)
        {
            AutoMode = AutoModeCheckButton.IsChecked.Value;

            MessageSB.Append("Auto Mode ");
            MessageSB.AppendLine(AutoMode.ToString());

            if (AutoMode)
                DoAutoMode();

        }
        #endregion

    }
}
