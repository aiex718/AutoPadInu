#pragma once
#include "pch.h"
#include "PuzzDraOperator.h"
#include "PuzzDraRoute.h"

namespace PuzzDraSolver
{
	extern Route beamSearch(BOARD& board, int aheads, int length, int beamWeight, int startRow, int startColumn, int& outScore);

	extern void beamSearchTest();
}
