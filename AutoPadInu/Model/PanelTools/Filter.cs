using OpenCVBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using OpenCVBridge.Models;
using Windows.Foundation;
using AutoPadInu_UWP.Model.Datatype;

namespace AutoPadInu_UWP.Model.PanelTools
{
    class Filter
    {
        public static List<Filter> AllFilter = new List<Filter>()
        {
            new Filter
            {
                BeadType = Beads.Notset,
                HsvRanges = new HsvRange[]
                {
                    new HsvRange(new HsvValue(0, 0, 0),new HsvValue(255, 255, 255))
                }
            },
            new Filter
            {
                BeadType = Beads.Fire,
                HsvRanges = new HsvRange[]
                {
                    new HsvRange(new HsvValue(0, 50, 0),new HsvValue(21, 255, 255)),
                    new HsvRange(new HsvValue(172, 50, 0),new HsvValue(180, 255, 255))
                }
            },
            new Filter
            {
                BeadType = Beads.Water,
                HsvRanges = new HsvRange[]
                {
                    new HsvRange(new HsvValue(89, 50, 0),new HsvValue(122, 255, 255))
                }
            },
            new Filter
            {
                BeadType = Beads.Wood,
                HsvRanges = new HsvRange[]
                {
                    new HsvRange(new HsvValue(58, 50, 0),new HsvValue(83, 255, 255))
                }
            },
            new Filter
            {
                BeadType = Beads.Light,
                HsvRanges = new HsvRange[]
                {
                    new HsvRange(new HsvValue(25, 50, 0),new HsvValue(58, 255, 255))
                }
            },
            new Filter
            {
                BeadType = Beads.Dark,
                HsvRanges = new HsvRange[]
                {
                    new HsvRange(new HsvValue(114, 50, 0),new HsvValue(140, 255, 255))
                }
            },
            new Filter
            {
                BeadType = Beads.Heart,
                HsvRanges = new HsvRange[]
                {
                    new HsvRange(new HsvValue(138, 50, 0),new HsvValue(167, 255, 255))
                }
            }
        };

        IEnumerable<HsvRange> HsvRanges;
        public Beads BeadType { get; private set; } = Beads.Notset;
        private OpenCVHelper _cvhelper = new OpenCVHelper();


        public PadPanel FiltePanel(SoftwareBitmap input, out SoftwareBitmap output)
        {
            output = new SoftwareBitmap(BitmapPixelFormat.Bgra8, input.PixelWidth, input.PixelHeight, BitmapAlphaMode.Premultiplied);

            List<Point> BeadLocations=new List<Point>();
            _cvhelper.ParsePanelByHsv(input, output, HsvRanges,PadPanel.size, BeadLocations);

            PadPanel p = new PadPanel();
            foreach (var point in BeadLocations)
                p.beads[(int)point.X, (int)point.Y] = BeadType;
            
            return p;
        }

        public bool IsPanelAvailable(SoftwareBitmap input,int Threshold)
        {
            _cvhelper.GetAvgBrightness(input, out double AvgBright);
            return AvgBright >= Threshold;
        }
    }
}
