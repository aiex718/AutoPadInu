#pragma once
#include "PuzzDraRoute.h"
#include "PuzzDraOperator.h"

using namespace std;
using namespace PuzzDraSolver;
using namespace Platform;
using namespace Windows::Foundation;

namespace PuzzDraSolver
{
	public ref class PuzzDraBeamParser sealed
	{
	public:
		PuzzDraBeamParser();
		void InitSearchData(int maxLookAhead);
		void BeamSearch(const Array<int, 1>^ boardArr, Point StartPoint, int aheads, int lenLimit, int beamWeight, const Array<int, 1>^ outRoute, int* outScore);
		void FirstSearch(const Array<int, 1>^ boardArr, Point StartPoint, int aheads, int lenLimit, int beamWeight, const Array<int, 1>^ outRoute, int* outScore);
		void MoveByRoute(const Array<int, 1>^ boardArr, const Array<int, 1>^ routeArr, Point StartPoint);

	private:
		Route firstSearch_4(BOARD & board, int lookAheadCount, int length, int startRow, int startColumn, int & outScore);
		Route beamSearch(BOARD & board, int aheads, int length, int beamWeight, int startRow, int startColumn, int & outScore);
		void calcRoutes_4(int lookAheadCount);

		BOARD toBoard(int * boardArr);
		Route toRoute(int * routeArr);
		void toBoardArr(BOARD & board, int * boardArr);
		void toRouteArr(const Route & route, int * routeArr);

		static const int LookAheadMax = 11;

		vector<Route> _routes[LookAheadMax];
	};
}

