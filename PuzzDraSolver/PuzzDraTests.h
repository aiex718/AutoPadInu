#pragma once
#include "pch.h"

#include "PuzzDraOperator.h"
#include "PuzzDraRoute.h"


namespace PuzzDraSolver
{
	extern void inputBoard(BOARD&);
	extern string toStringBoard(BOARD&);
	extern string toStringRoute(Route);
	extern void debug(BOARD& board);
	extern void debug(Route route);

	extern void firstSearchTest();
	extern vector<vector<int>> generateRandomBoard();

	extern int fillBoardRandom(BOARD& board);
}
