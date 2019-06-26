using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPadInu_UWP.Model.Tools
{
    class Countdown
    {
        int Duration = -1;
        bool AutoReset = false;
        DateTime PreviousTime = DateTime.MinValue;

        public Countdown(int duration)
        {
            Duration = duration;
        }

        public Countdown(int duration, bool autoReset)
        {
            Duration = duration;
            AutoReset = autoReset;
        }

        public bool isTimeout()
        {
            if (Duration <= 0)
            {
                return true;
            }
            else if (DateTime.Now - PreviousTime > TimeSpan.FromMilliseconds(Duration))
            {
                if (AutoReset)
                {
                    Reset();
                }

                return true;
            }

            return false;
        }

        public void Reset()
        {
            PreviousTime = DateTime.Now;
        }
    }
}
