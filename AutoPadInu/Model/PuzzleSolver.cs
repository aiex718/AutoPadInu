using AutoPadInu_UWP.Model;
using AutoPadInu_UWP.Model.Datatype;
using PuzzDraSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AutoPadInu_UWP.Model
{
    class PuzzleSolver
    {
        const int MaxLookAhead = 4;

        public static Route SolvePuzzleLoop(PadPanel padPanel, int lenLimit, int TargetScore)
        {
            Route route = null;
            for (int ahead = 1; ahead < MaxLookAhead; ahead++)
            {
                //route = SolvePuzzle(padPanel, ahead, lenLimit, ahead,TargetScore, false);
                //if (route.Score >= TargetScore)
                //{
                //    route.Message = "Ahead=" + ahead + ",Deep mode=false";
                //    break;
                //}

                route = SolvePuzzle(padPanel, ahead, lenLimit, ahead, TargetScore, true);
                if (route.Score >= TargetScore)
                {
                    route.Message = "Ahead=" + ahead + ",Deep mode=true";
                    break;
                }
            }
            return route;
        }

        public static Route SolvePuzzle(PadPanel padPanel, int aheads, int lenLimit, int beamWeight,int TargetScore,bool DeepMode=false)
        {
            List<Route> Routes = new List<Route>();
            Route TargetRoute = null;

            var v = Parallel.For(0, PadPanel.Height, (y,state) =>
            {
                for (int x = 0; x < PadPanel.Width; x++)
                {
                    PuzzDraBeamParser puzzDraSolver = new PuzzDraBeamParser();
                    puzzDraSolver.InitSearchData(aheads);

                    int[] outRouteArr = new int[lenLimit];
                    Point StartPoint = new Point(x, y);

                    int Score=0;
                    if(DeepMode)
                        puzzDraSolver.BeamSearch(padPanel.ToBoardArr(), StartPoint, aheads, lenLimit, beamWeight, outRouteArr,out Score);
                    else
                        puzzDraSolver.FirstSearch(padPanel.ToBoardArr(), StartPoint, aheads, lenLimit, beamWeight, outRouteArr, out Score);

                    Route r = new Route(outRouteArr, StartPoint, Score);

                    lock (Routes)
                        Routes.Add(r);

                    if (r.Score>= TargetScore)
                    {
                        TargetRoute = r;
                        state.Break();
                    }

                }
            });
            
            return TargetRoute ?? Routes.OrderByDescending(r => r.Score).First();
        }

    }
}
