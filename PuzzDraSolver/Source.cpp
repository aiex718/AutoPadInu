#include "PuzzDraOperator.h"
#include "PuzzDraRoute.h"
#include "PuzzDraTests.h"
#include "PuzzDraBeamSearch.h"
#include "PuzzDraFirstSearch.h"

#include "Source.h"

using namespace PuzzDraSolver;

Solver::Solver()
{

}

BOARD Solver::toBoard(int* boardArr)
{
	vector<vector<int>> board(HEIGHT, vector<int>(WIDTH));
	for (int i = 0; i < DROP_NUM; i++)
	{
		int r = i / WIDTH, c = i % WIDTH;
		board[r][c] = boardArr[i];
	}
	return std::move(board);
}

Route Solver::toRoute(int* routeArr)
{
	Route route;
	for (int i = 0; ; i++)
	{
		if (routeArr[i] == -1)
			break;
		route.push_back(routeArr[i]);
	}
	return route;
}

void Solver::toRouteArr(const Route& route, int* routeArr)
{
	for (int i = 0; i < route.size(); i++)
	{
		routeArr[i] = route.getBits(i);
	}

	routeArr[route.size()] = -1;
}

void Solver::toBoardArr(BOARD& board, int* boardArr)
{
	for (int i = 0; i < HEIGHT; i++)
	{
		for (int j = 0; j < WIDTH; j++)
		{
			boardArr[i * WIDTH + j] = board[i][j];
		}
	}
}

//void Solver::InitializeSearchData(int maxLookAhead)
//{
//	if (maxLookAhead >= LookAheadMax) return;
//	calcRoutes_4(maxLookAhead);
//}

int Solver::BeamSearchAuto(const Array<int, 1>^ boardArr, int aheads, int length, int beamWeight, const Array<int, 1>^ outRoute,int* outRow, int* outColumn, int* outScore)
{
	BOARD board = toBoard(boardArr->Data);

	int evalMax = -1;
	Route ansRoute;
	int row = 0, column = 0;
	for (int i = 0; i < HEIGHT; i++)
	{
		for (int j = 0; j < WIDTH; j++)
		{
			auto tmpBoard = board;
			int evalNum;
			Route route = beamSearch(tmpBoard, aheads, length, beamWeight, i, j, evalNum);

			if (evalNum > evalMax)
			{
				evalMax = evalNum;
				ansRoute = route;
				row = i;
				column = j;
			}
		}
	}

	*outRow = row;
	*outColumn = column;
	*outScore = evalMax;


	toRouteArr(ansRoute, outRoute->Data);

	return evalMax;
}

int Solver::BeamSearch(const Array<int,1>^ boardArr , int aheads, int length, int beamWeight, int startRow, int startColumn, const Array<int,1>^ outRoute)
{
	BOARD board = toBoard(boardArr->Data);

	int score;
	auto route = beamSearch(board, aheads, length, beamWeight, startRow, startColumn, score);

	toRouteArr(route, outRoute->Data);

	return score;
}

int Solver::FirstSearch(const  Array<int,1>^ boardArr, int aheads, int length, int startRow, int startColumn,const Array<int,1>^ outRoute)
{
	BOARD board = toBoard(boardArr->Data);

	int score;
	auto route = firstSearch_4(board, aheads, length, startRow, startColumn, score);

	toRouteArr(route, outRoute->Data);

	return score;
}

int Solver::EvalCombo(const  Array<int,1>^ boardArr)
{
	BOARD board = toBoard(boardArr->Data);
	int comboCount = evalCombo(board);
	toBoardArr(board, boardArr->Data);

	return comboCount;
}

int Solver::CountReach(const  Array<int,1>^ boardArr)
{
	BOARD board = toBoard(boardArr->Data);
	int reachCount = countReach(board);
	toBoardArr(board, boardArr->Data);

	return reachCount;
}

int Solver::EvalProcess(const  Array<int,1>^ boardArr)
{
	BOARD board = toBoard(boardArr->Data);
	int score = evalProcess(board);
	toBoardArr(board, boardArr->Data);

	return score;
}

int Solver::PushDownDrops(const Array<int,1>^ boardArr)
{
	BOARD board = toBoard(boardArr->Data);
	bool pushed = pushDownDrops(board);
	toBoardArr(board, boardArr->Data);

	//DllImport‚ÌŠÖŒW‚Åint‚Å•Ô‚·
	return pushed;
}

int Solver::FillBoardRandom(const  Array<int,1>^ boardArr)
{
	BOARD board = toBoard(boardArr->Data);
	int score = fillBoardRandom(board);
	toBoardArr(board, boardArr->Data);
	return score;
}

void Solver::MoveByRoute(const  Array<int,1>^ boardArr, const  Array<int,1>^ routeArr, int startRow, int startColumn)
{
	BOARD board = toBoard(boardArr->Data);
	Route route = toRoute(routeArr->Data);
	moveByRoute(board, route, startRow, startColumn);
	toBoardArr(board, boardArr->Data);
}


//int main(void)
//{
//	calcRoutes_4(10);
//	while (true)
//	{
//		//outputAnswer();
//		//simplifyRouteTest();
//		
//		firstSearchTest();
//		beamSearchTest();
//	}
//	return EXIT_SUCCESS;
//}
