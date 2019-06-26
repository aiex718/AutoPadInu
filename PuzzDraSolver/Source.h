#pragma once


using namespace Platform;

namespace PuzzDraSolver
{
	public ref class Solver sealed
	{
	public:			
		Solver();
		//void InitializeSearchData(int maxLookAhead);
		int BeamSearchAuto(const Array<int, 1>^ boardArr, int aheads, int length, int beamWeight, const Array<int, 1>^ outRoute, int* outRow, int* outColumn, int* outScore);
		int BeamSearch(const Array<int,1>^ boardArr, int aheads, int length, int beamWeight, int startRow, int startColumn, const Array<int,1>^ outRoute);
		int FirstSearch(const Array<int,1>^ boardArr, int aheads, int length, int startRow, int startColumn, const Array<int,1>^ outRoute);
		int EvalCombo(const Array<int,1>^ boardArr);
		int CountReach(const Array<int,1>^ boardArr);
		int EvalProcess(const Array<int,1>^ boardArr);
		int PushDownDrops(const Array<int,1>^ boardArr);
		int FillBoardRandom(const Array<int,1>^ boardArr);
		void MoveByRoute(const Array<int,1>^ boardArr, const Array<int,1>^ routeArr, int startRow, int startColumn);

	private:
		void toRouteArr(const Route& route, int* routeArr);
		void toBoardArr(BOARD& board, int* boardArr);

		BOARD toBoard(int* boardArr);
		Route toRoute(int* routeArr);
	};

}
