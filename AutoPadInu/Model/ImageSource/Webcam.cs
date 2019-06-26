using AutoPadInu_UWP.Model.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;

namespace AutoPadInu_UWP.Model.ImageSource
{
    class Webcam
    {
        public bool Enable { get; set; }

        MediaCapture _mediaCapture = null;
        MediaFrameReader _reader = null;

        Countdown CaptureCD = null;

        const int IMAGE_ROWS = 480;
        const int IMAGE_COLS = 640;

        public delegate void OnReceiveFrameHandler(SoftwareBitmap softwareBitmap);
        
        public event OnReceiveFrameHandler OnReceiveFrame;

        public Webcam(int CaptureIntervalMs)
        {
            CaptureCD = new Countdown(CaptureIntervalMs, false);
        }

        private async Task InitializeMediaCaptureAsync(MediaFrameSourceGroup sourceGroup)
        {
            if (_mediaCapture != null)
            {
                return;
            }

            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings()
            {
                SourceGroup = sourceGroup,
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };
            await _mediaCapture.InitializeAsync(settings);
        }

        public async Task Initialize()
        {
            // Find the sources 
            var allGroups = await MediaFrameSourceGroup.FindAllAsync();
            var sourceGroups = allGroups.Select(g => new
            {
                Group = g,
                SourceInfo = g.SourceInfos.FirstOrDefault(i => i.SourceKind == MediaFrameSourceKind.Color)
            }).Where(g => g.SourceInfo != null).ToList();

            if (sourceGroups.Count == 0)
            {
                // No camera sources found
                return;
            }
            var selectedSource = sourceGroups.FirstOrDefault();

            // Initialize MediaCapture
            try
            {
                await InitializeMediaCaptureAsync(selectedSource.Group);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("MediaCapture initialization error: " + exception.Message);
                await Cleanup();
                return;
            }

            // Create the frame reader
            MediaFrameSource frameSource = _mediaCapture.FrameSources[selectedSource.SourceInfo.Id];
            BitmapSize size = new BitmapSize() // Choose a lower resolution to make the image processing more performant
            {
                Height = IMAGE_ROWS,
                Width = IMAGE_COLS
            };

            _reader = await _mediaCapture.CreateFrameReaderAsync(frameSource, MediaEncodingSubtypes.Bgra8, size);
            _reader.FrameArrived += ColorFrameReader_FrameArrivedAsync;
            await _reader.StartAsync();
        }

        private void ColorFrameReader_FrameArrivedAsync(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            if (Enable && CaptureCD.isTimeout())
            {
                var frame = sender.TryAcquireLatestFrame();
                if (frame != null)
                {
                    var inputBitmap = frame.VideoMediaFrame?.SoftwareBitmap;
                    if (inputBitmap != null)
                    {
                        OnReceiveFrame?.Invoke(inputBitmap);
                    }
                    //inputBitmap.Dispose();
                }
                CaptureCD.Reset();
            }
        }


        public async Task Cleanup()
        {
            if (_mediaCapture != null)
            {
                await _reader.StopAsync();
                _reader.FrameArrived -= ColorFrameReader_FrameArrivedAsync;
                _reader.Dispose();
                _mediaCapture = null;
            }
        }

    }
}
