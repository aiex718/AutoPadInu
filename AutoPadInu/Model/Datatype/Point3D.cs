using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPadInu_UWP.Model.Datatype
{
    class Point3D
    {
        public Point3D(double x,double y,double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public double X;
        public double Y;
        public double Z;

        public bool Equals(Point3D p)
        {
            return X == p.X && Y == p.Y & Z == p.Z;
        }
    }
}
