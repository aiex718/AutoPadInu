using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AutoPadInu_UWP.Model.Datatype
{
    class PhoneConfig
    {
        public PhoneConfig(Size sizemm, Point3D originPoint,Rect padPanelLocation,int liftZmm)
        {
            PanelSizeMM = sizemm;
            OriginPoint = originPoint;
            InitLiftZmm = liftZmm;
            PadPanelLocation = padPanelLocation;
        }

        readonly public Size PanelSizeMM;
        readonly public Point3D OriginPoint;
        readonly public int InitLiftZmm;
        readonly public Rect PadPanelLocation;
    }

    class PhoneConfigs
    {
        public static PhoneConfig iPhone6s = 
            new PhoneConfig( 
                  new Size(57, 48)
                , new Point3D(29.6, 57, 6)
                , new Rect(new Point(129, 338), new Point(347, 526))
                , 17);
    }
}
