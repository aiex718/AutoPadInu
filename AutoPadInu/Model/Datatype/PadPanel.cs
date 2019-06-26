using PuzzDraSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AutoPadInu_UWP.Model.Datatype
{
    enum Beads
    {
        Notset,
        Fire,
        Water,
        Wood,
        Light,
        Dark,
        Heart
    }

    class PadPanel
    {
        static Solver puzzDraSolver = new Solver();

        public const int Width=6;
        public const int Height = 5;
        public static readonly Size size = new Size(Width,Height);

        public Beads[,] beads = new Beads[Width, Height];

        public PadPanel()
        {

        }

        public PadPanel(int[] BoardArr)
        {
            for (int y = 0; y < PadPanel.Height; y++)
            {
                for (int x = 0; x < PadPanel.Width; x++)
                    beads[x, y] = (Beads)BoardArr[y * PadPanel.Width + x];
            }
        }

        public int[] ToBoardArr()
        {
            int[] Result = new int[PadPanel.Height * PadPanel.Width];
            for (int y = 0; y < PadPanel.Height; y++)
            {
                for (int x = 0; x < PadPanel.Width; x++)
                    Result[y* PadPanel.Width+x] = (int)beads[x, y];
            }

            return Result;
        }

        public bool IsValid()
        {
            foreach (var item in beads)
            {
                if (item == Beads.Notset)
                    return false;
            }
            return true;
        }

        public PadPanel ApplyRoute(Route route)
        {
            var BoardArr = this.ToBoardArr();
            puzzDraSolver.MoveByRoute(BoardArr, route.ToRouteArr(), (int)route.StartPoint.Y, (int)route.StartPoint.X);

            return new PadPanel(BoardArr);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < PadPanel.Height; y++)
            {
                for (int x = 0; x < PadPanel.Width; x++)
                {
                    switch (beads[x, y])
                    {
                        case Beads.Notset:
                            sb.Append("？ ");
                            break;
                        case Beads.Fire:
                            sb.Append("火 ");
                            break;
                        case Beads.Water:
                            sb.Append("水 ");
                            break;
                        case Beads.Wood:
                            sb.Append("木 ");
                            break;
                        case Beads.Light:
                            sb.Append("光 ");
                            break;
                        case Beads.Dark:
                            sb.Append("暗 ");
                            break;
                        case Beads.Heart:
                            sb.Append("心 ");
                            break;
                        default:
                            break;
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
