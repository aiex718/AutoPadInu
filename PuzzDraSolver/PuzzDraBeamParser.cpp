#include "PuzzDraBeamParser.h"



PuzzDraBeamParser::PuzzDraBeamParser()
{

}

void PuzzDraBeamParser::InitSearchData(int maxLookAhead)
{
	if (maxLookAhead >= LookAheadMax) 
		return;
	calcRoutes_4(maxLookAhead);
}

void PuzzDraBeamParser::BeamSearch(	const Array<int, 1>^ boardArr,Point StartPoint,int aheads,int lenLimit,int beamWeight,const Array<int,1>^ outRoute,int * outScore)
{
	BOARD board = toBoard(boardArr->Data);

	//int score;
	auto route = beamSearch(board, aheads, lenLimit, beamWeight, (int)StartPoint.Y, (int)StartPoint.X, *outScore);

	toRouteArr(route, outRoute->Data);

	//*outScore = score;
}

void PuzzDraBeamParser::FirstSearch(const Array<int, 1>^ boardArr, Point StartPoint, int aheads, int lenLimit, int beamWeight, const Array<int, 1>^ outRoute, int * outScore)
{
	BOARD board = toBoard(boardArr->Data);

	//int score;
	auto route = firstSearch_4(board, aheads, lenLimit, (int)StartPoint.Y, (int)StartPoint.X, *outScore);

	toRouteArr(route, outRoute->Data);

}

void PuzzDraBeamParser::MoveByRoute(const Array<int, 1>^ boardArr, const Array<int, 1>^ routeArr, Point StartPoint)
{
	BOARD board = toBoard(boardArr->Data);
	Route route = toRoute(routeArr->Data);
	moveByRoute(board, route, StartPoint.Y, StartPoint.X);
	toBoardArr(board, boardArr->Data);
}


Route PuzzDraBeamParser::firstSearch_4(BOARD& board, int lookAheadCount, int length, int startRow, int startColumn, int& outScore)
{
	if (outOfRange(startRow, startColumn)) return Route();

	Route addedRoute;
	Route ansRoute;
	int evalAns = -1;
	//if (_routes[lookAheadCount].empty())
	//	calcRoutes_4(lookAheadCount);

	//int evalAllMax = -1;
	//int preSize = -1;
	for (int i = 0; i < length; i++)
	{
		Route nextRoute = addedRoute;
		int evalMax = -1;
		int aheads = lookAheadCount;
		if (i + lookAheadCount >= length)
		{
			aheads = length - i - 1;
		}
		const int size = _routes[aheads].size();
		for (int j = 0; j < size; j++)
		{
			Route& route = _routes[aheads][j];

			vector<vector<int>> tmpBoard = board;
			Route tmpRoute = addedRoute;
			tmpRoute.push_back(route);

			//追加するルートの一回目の移動が意味のないものである場合は飛ばす
			const int top = addedRoute.top();
			if (top != -1 && (top + 2) % 4 == route.getBits(0) && size >= 4)
			{
				j += size / 4 - 1;
				continue;
			}

			//単純化出来る道を探索しないようにしても意味なかった(精度が落ちた)
			//Route simRoute;
			//simplifyRoute(board, tmpRoute, startRow, startColumn, simRoute);
			//if (tmpRoute.size() != simRoute.size())
			//{
			//	continue;
			//}

			//debug(route);
			//debug(tmpRoute);

			moveByRoute(tmpBoard, tmpRoute, startRow, startColumn);

			//debug(tmpBoard);

			int evalNum = evalProcess(tmpBoard);

			//cout << evalNum << endl;

			if (evalNum > evalMax)
			{
				nextRoute = tmpRoute;
				evalMax = evalNum;
			}
		}

		//山登りにすると減った
		//if (evalAllMax > evalMax) break;
		//evalAllMax = evalMax;

		//やっていいのか微妙
		//if (nextRoute.size() == preSize)
		//{
		//	break;
		//}
		//preSize = nextRoute.size();

		//一番いいのを取る
		if (evalMax > evalAns)
		{
			ansRoute = nextRoute;
			evalAns = evalMax;
		}
		//else if(evalMax < evalAns) break;
		//山降りる

		int routeBits = nextRoute.getBits(nextRoute.size() - aheads);

		//lengthまでの探索が終わっている
		if (i + lookAheadCount >= length)
		{
			//addedRoute = nextRoute;
			break;
		}

		addedRoute.push_back(routeBits);

		//cout << i << endl;
		//debug(nextRoute);
		//debug(addedRoute);
	}
	outScore = evalAns;
	return ansRoute;
}

Route PuzzDraBeamParser::beamSearch(BOARD& board, int aheads, int length, int beamWeight, int startRow, int startColumn, int& outScore)
{
	std::vector<Route> preRoutes(beamWeight);
	std::priority_queue<std::pair<int, Route>> ordered;
	for (const auto& route : _routes[1])
		ordered.emplace(0, route);

	Route ansRoute;
	int evalMax = -1;

	length = min(length, MAX_ROUTE_SIZE);//

	//auto at = _routes[aheads][0];
	int loopCount = (length - 1) / _routes[aheads][0].size() + !!((length - 1) % aheads);//length???
	for (int k = 0; k < loopCount; k++)
	{
		preRoutes.clear();
		for (int i = 0; i < beamWeight && !ordered.empty(); i++)
		{
			preRoutes.push_back(ordered.top().second);
			ordered.pop();
		}

		for (int i = 1; i <= aheads; i++)
		{
			const int size = _routes[i].size();
			for (const auto& route : preRoutes)
			{
				for (int j = 0; j < size; j++)
				{
					const auto& added = _routes[i][j];

					BOARD tmpBoard = board;
					Route next = route;
					Route dummy;

					const int rs = route.size();
					const int as = added.size();
					if (rs + as > length) continue;//

					////????
					////?
					//const int top = added.top();
					//if (top != -1 && (top + 2) % 4 == route.getBits(0) && size >= 4)
					//{
					//	j += size / 4 - 1;
					//	continue;
					//}

					next.push_back(added);
					if (simplifyRoute(next, startRow, startColumn, dummy) > 0)
						continue;

					int ms = next.size();
					moveByRoute(tmpBoard, next, startRow, startColumn);
					int evalNum = evalProcess(tmpBoard);

					if (evalNum > evalMax)
					{
						evalMax = evalNum;
						ansRoute = next;
					}
					ordered.emplace(evalNum, next);
				}
			}
		}
	}
	outScore = evalMax;
	return ansRoute;
}

void PuzzDraBeamParser::calcRoutes_4(int lookAheadCount)
{
	//route, length
	std::queue<std::tuple<Route, int>> que;
	que.emplace(Route(), 0);

	for (int i = 0; i < LookAheadMax; i++)
	{
		_routes[i].clear();
	}

	//bfs??
	while (!que.empty())
	{
		auto tup = que.front(); que.pop();
		int length = std::get<1>(tup);
		Route route = std::get<0>(tup);

		_routes[length].push_back(route);

		//debug(route);

		if (length == lookAheadCount)
		{
			continue;
		}

		int pre = route.top();
		for (int i = 0; i < 4; i++)
		{
			if ((i + 2) % 4 == pre) continue;//

			Route nextRoute = route;
			nextRoute.push_back(i);
			que.emplace(nextRoute, length + 1);

		}
	}
}


BOARD PuzzDraBeamParser::toBoard(int* boardArr)
{
	vector<vector<int>> board(HEIGHT, vector<int>(WIDTH));
	for (int i = 0; i < DROP_NUM; i++)
	{
		int r = i / WIDTH, c = i % WIDTH;
		board[r][c] = boardArr[i];
	}
	return std::move(board);
}

Route PuzzDraBeamParser::toRoute(int* routeArr)
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

void PuzzDraBeamParser::toRouteArr(const Route& route, int* routeArr)
{
	for (int i = 0; i < route.size(); i++)
	{
		routeArr[i] = route.getBits(i);
	}

	routeArr[route.size()] = -1;
}

void PuzzDraBeamParser::toBoardArr(BOARD& board, int* boardArr)
{
	for (int i = 0; i < HEIGHT; i++)
	{
		for (int j = 0; j < WIDTH; j++)
		{
			boardArr[i * WIDTH + j] = board[i][j];
		}
	}
}