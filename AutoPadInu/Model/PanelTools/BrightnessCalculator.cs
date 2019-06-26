using OpenCVBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace AutoPadInu_UWP.Model.PanelTools
{
    
    class BrightnessCalculator
    {
        public enum Edges { Pos,Neg}

        const int SampleLimit = 10;

        List<double> Samples = new List<double>();
        public double? Threshold { get; private set; } = null;
        public double? Latest { get; private set; } = null;

        OpenCVHelper _cvhelper = new OpenCVHelper();

        public double CalcBrightness(SoftwareBitmap softwareBitmap)
        {
            _cvhelper.GetAvgBrightness(softwareBitmap, out double val);

            if (Threshold == null)
            {
                Samples.Add(val);
                if (Samples.Count >= SampleLimit)
                    Threshold = (int)(Samples.Average() * 0.85);
            }

            Latest = val;
            return val;
        }

        public void Clear()
        {
            Threshold = null;
            Samples.Clear();
        }

        public void WaitUntilEdge(int Timeout, Edges Edge)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var token = cts.Token;
                try
                {
                    Task t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (Edge == Edges.Neg)
                            {
                                while (Latest >= Threshold)
                                {
                                    token.ThrowIfCancellationRequested();
                                    Task.Delay(10).Wait();
                                }
                            }
                            else if (Edge == Edges.Pos)
                            {
                                while (Latest <= Threshold)
                                {
                                    token.ThrowIfCancellationRequested();
                                    Task.Delay(10).Wait();
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    });

                    Task delay = Task.Delay(Timeout, token);

                    Task.WaitAny(new Task[] { t,delay});

                    cts.Cancel();
                }
                catch (Exception)
                {
                    
                }

            }
        }


    }
}
