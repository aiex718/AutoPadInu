using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AutoPadInu_UWP.Model.Datatype
{
    enum Direction
    {
        Up,Right,Down,Left
    }

    class Route
    {
        public Point StartPoint;
        public List<Direction> Directions ;
        public int Length { get { return Directions.Count; } }
        public int Score { get; private set; }
        public string Message;

        public Route()
        {
            Directions = new List<Direction>();
        }

        public Route(IEnumerable<int> RouteArr, Point startPoint,int score)
        {
            Directions = new List<Direction>();
            foreach (var dir in RouteArr)
            {
                if (dir == -1)
                    break;
                else
                    Directions.Add((Direction)dir);
            }

            StartPoint = startPoint;
            Score = score;
        }

        public int[] ToRouteArr()
        {
            Directions.Add((Direction)(-1));
            int[] Result = Directions.Cast<int>().ToArray();
            Directions.RemoveAt(Directions.Count-1);//remove last
            return Result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Start Point:");
            sb.AppendLine(StartPoint.ToString());
            sb.Append("Score:");
            sb.AppendLine(Score.ToString());
            sb.Append("Len:");
            sb.AppendLine(Length.ToString());
            sb.Append("Direction:");

            foreach (var dir in Directions)
            {
                switch (dir)
                {
                    case Direction.Up:
                        sb.Append('↑');
                        break;
                    case Direction.Right:
                        sb.Append('→');
                        break;
                    case Direction.Down:
                        sb.Append('↓');
                        break;
                    case Direction.Left:
                        sb.Append('←');
                        break;
                    default:
                        break;
                }
            }
            sb.AppendLine();
            sb.Append("Msg:");
            sb.AppendLine(Message);

            return sb.ToString();
        }
    }
}
