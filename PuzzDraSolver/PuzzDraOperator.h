#pragma once
#include "pch.h"

#include <vector>
#include <utility>
#include <functional>
#include <iostream>


namespace PuzzDraSolver
{

	using std::vector;
	using std::string;
	using std::cin;
	using std::cout;
	using std::endl;

	const int WIDTH = 6;
	const int HEIGHT = 5;
	const int DROP_NUM = WIDTH * HEIGHT;
	const int ATTRIBUTE_NUM = 6;
	const int LEAST_COMBO_NUM = 3;
	const int EMPTY = 0;

	typedef vector<vector<int>> BOARD;

	//�R?�{����p�Ɏg��
	//http://www.prefield.com/algorithm/container/union_find.html
	struct UnionFind {
		std::vector<int> data;
		UnionFind(int size) : data(size, -1) { }
		bool unionSet(int x, int y) {
			x = root(x); y = root(y);
			if (x != y) {
				if (data[y] < data[x]) std::swap(x, y);
				data[x] += data[y]; data[y] = x;
			}
			return x != y;
		}
		bool findSet(int x, int y) {
			return root(x) == root(y);
		}
		int root(int x) {
			return data[x] < 0 ? x : data[x] = root(data[x]);
		}
		int size(int x) {
			return -data[root(x)];
		}
	};


	//�͈̓`�F�b�N�̃w?�p��?
	//0 <= row < HEIGHT && 0 <= column < WIDTH
	inline bool outOfRange(int row, int column)
	{
		return row < 0 || row >= HEIGHT || column < 0 || column >= WIDTH;
	}


	inline void swapDrop(BOARD& board, int row1, int column1, int row2, int column2)
	{
		std::swap(board[row1][column1], board[row2][column2]);
	}


	const int _dr_4[] = { -1, 0, 1, 0 };
	const int _dc_4[] = { 0, 1, 0, -1 };

	//4�ߖT�Ƀh?�b�v���ړ�
	//direction 0����ɂ���1����90�x?�v���
	//�ړ���̃h?�b�v�̈��Ԃ�(0~2bit�ڂ�row, 3~5bit�ڂ�column)
	inline int slideDrop_4(BOARD& board, int row, int column, int direction)
	{
		if (direction < 0 || direction >= 4) return (column << 3) | row;

		int nextRow = row + _dr_4[direction];
		int nextColumn = column + _dc_4[direction];
		if (outOfRange(nextRow, nextColumn)) return (column << 3) | row;

		swapDrop(board, row, column, nextRow, nextColumn);
		return (nextColumn << 3) | nextRow;
	}


	const int _dr_8[] = { -1, -1, 0, 1, 1, 1, 0, -1 };
	const int _dc_8[] = { 0, 1, 1, 1, 0, -1, -1, -1 };

	//8�ߖT�Ƀh?�b�v���ړ�
	//direction 0����ɂ���1����45�x?�v���
	inline void slideDrop_8(BOARD& board, int row, int column, int direction)
	{
		if (direction < 0 || direction >= 8) return;

		int nextRow = row + _dr_8[direction];
		int nextColumn = column + _dc_8[direction];
		if (outOfRange(nextRow, nextColumn)) return;

		swapDrop(board, row, column, nextRow, nextColumn);
	}



	inline bool pushDownDrops(BOARD& board)
	{
		bool changed = false;
		for (int column = 0; column < WIDTH; column++)//���ꂼ��̍s�ɂ���
		{
			int empties = 0;//��̃h?�b�v��?
			for (int row = HEIGHT - 1; row >= 0; row--)//������
			{
				if (board[row][column] != EMPTY)
				{
					board[row + empties][column] = board[row][column];//����

					if (empties != 0)
					{
						board[row][column] = 0;
						changed = true;
					}
				}
				else
				{
					empties++;
				}
			}
		}

		return changed;
	}



	//�ƂȂ�ɃR?�{��?��΂���ɂȂ���
	inline void checkConnect(BOARD& board, UnionFind& comboTree, int pos, int adjacentPos)
	{
		if (adjacentPos < 0 || adjacentPos >= DROP_NUM) return;

		int root = comboTree.root(adjacentPos);

		//�R?�{�ɑ����Ă���
		if (root != adjacentPos)
		{
			//����
			int row1 = pos / WIDTH;
			int column1 = pos % WIDTH;
			int row2 = root / WIDTH;
			int column2 = root % WIDTH;

			//�����������Ȃ�Ȃ���
			if (board[row1][column1] == board[row2][column2])
			{
				comboTree.unionSet(adjacentPos, pos);
			}
		}
	}


	//�c�̃R?�{
	inline void comboRow(BOARD& board, UnionFind& comboTree, int row, int column)
	{
		//�R?�{�ɂȂ�Ȃ�
		if (row + 2 >= HEIGHT ||
			board[row][column] != board[row + 1][column] || board[row][column] != board[row + 2][column])
			return;


		int pos = row * WIDTH + column;//���R��

		checkConnect(board, comboTree, pos, pos - WIDTH);//��i��
		checkConnect(board, comboTree, pos, pos - 1);//����
		checkConnect(board, comboTree, pos, pos + 1);//�E��

		comboTree.unionSet(pos, pos + WIDTH);//���
		comboTree.unionSet(pos, pos + WIDTH + WIDTH);//���
	}


	//���̃R?�{
	inline void comboColumn(BOARD& board, UnionFind& comboTree, int row, int column)
	{
		//�R?�{�ɂȂ�Ȃ�
		if (column + 2 >= WIDTH ||
			board[row][column] != board[row][column + 1] || board[row][column] != board[row][column + 2])
			return;


		int pos = row * WIDTH + column;//���R��

		checkConnect(board, comboTree, pos, pos - WIDTH);//��i��
		checkConnect(board, comboTree, pos, pos - 1);//����
		checkConnect(board, comboTree, pos, pos + 1);//�E��

		comboTree.unionSet(pos, pos + 1);//��E
		comboTree.unionSet(pos, pos + 2);//��E
	}


	//�{�[�h��?�����ăR?�{�֌W���?����UnionFind���\�z���܂�
	inline void buildComboTree(BOARD& board, UnionFind& comboTree)
	{
		for (int row = 0; row < HEIGHT; row++)
		{
			for (int column = 0; column < WIDTH; column++)
			{
				if (board[row][column] == EMPTY) continue;

				//�c�̃R?�{
				comboRow(board, comboTree, row, column);
				//���̃R?�{
				comboColumn(board, comboTree, row, column);
			}
		}
	}


	//UnionFind�̏�񂩂�R?�{����?���A���łɃR?�{?���W�v���܂��B
	inline void removeCombo(BOARD& board, UnionFind& comboTree, int& out_comboCount)
	{
		int pos = 0;
		int combo = 0;
		for (int row = 0; row < HEIGHT; row++)
		{
			for (int column = 0; column < WIDTH; column++)
			{
				//�R?�{�ɑ����Ă���
				if (comboTree.root(pos) != pos)
				{
					board[row][column] = 0;
				}
				//�R?�{�̊�?�_
				else if (comboTree.size(pos) >= 3)
				{
					board[row][column] = 0;
					combo++;
				}

				pos++;
			}
		}
		out_comboCount = combo;
	}


	//�J�X�^?�ł���evalCombo
	//evaluationFunction����BOARD��UnionFind��ύX���Ă͂����Ȃ�
	inline int evalComboCustomized(BOARD& board, std::function<int(BOARD&, UnionFind&)> evaluationFunction)
	{
		UnionFind comboTree(DROP_NUM);
		buildComboTree(board, comboTree);

		int evaluatedValue = evaluationFunction(board, comboTree);

		int comboCount;
		removeCombo(board, comboTree, comboCount);
		return evaluatedValue;
	}


	inline int evalCombo(BOARD& board)
	{
		//���ォ��?�����Ă����A
		//�����Ă���_����ԍ���Ƃ���R?�{��g�ݗ��Ă�B
		//�ォ���ɗאڂ��Ă���R?�{������������?���Union-Find�؂��g����??����

		UnionFind comboTree(DROP_NUM);
		buildComboTree(board, comboTree);

		int comboCount;
		removeCombo(board, comboTree, comboCount);

		return comboCount;
	}


	//������ �u�҂��v
	//����̐F��?����R?�{��?����p�^�[?��?����
	//?�[�`��?
	inline int countReach(BOARD& board)
	{
		int reachCount = 0;
		for (int column = 0; column < WIDTH; column++)
		{
			for (int row = 0; row < HEIGHT - 1; row++)//�Ō�̒i�̈��܂�
			{
				if (board[row][column] != EMPTY) break;//�ォ�猩�ċ󂶂�Ȃ��Ȃ�����̗�͏I���
				if (board[row + 1][column] == EMPTY) continue;//������󂫂�������??���Ȃ�

				/*?�[�`�̃p�^�[?�͈ȉ���4��
				���̃R?�{�̈�ԉE�ɂȂ�
				���̃R?�{�̐^��?�ɂȂ�
				���̃R?�{�̈�ԍ��ɂȂ�
				�c�̃R?�{�̈�ԏ�ɂȂ�
				�R�[�h�d��?�o�C
				*/
				vector<bool> attributes(ATTRIBUTE_NUM + 1);//���̑�����?�[�`�ɂȂ��Ă��邩�ǂ������?����
				int counter = 0;
				if (column - 2 >= 0 && board[row][column - 2] != EMPTY &&
					board[row][column - 2] == board[row][column - 1] &&
					!attributes[board[row][column - 2]])//���łɂ��̑�����?�[�`�ɂȂ��Ă��Ȃ�
				{
					counter++;
					attributes[board[row][column - 2]] = true;
				}

				if (column - 1 >= 0 && column + 1 < WIDTH && board[row][column - 1] != EMPTY &&
					board[row][column - 1] == board[row][column + 1] &&
					!attributes[board[row][column - 1]])//���łɂ��̑�����?�[�`�ɂȂ��Ă��Ȃ�
				{
					counter++;
					attributes[board[row][column - 1]] = true;
				}

				if (column + 2 < WIDTH && board[row][column + 2] != EMPTY &&
					board[row][column + 1] == board[row][column + 2] &&
					!attributes[board[row][column + 2]])//���łɂ��̑�����?�[�`�ɂȂ��Ă��Ȃ�
				{
					counter++;
					attributes[board[row][column + 2]] = true;
				}

				if (row + 2 < HEIGHT &&
					board[row + 1][column] == board[row + 2][column] &&
					!attributes[board[row + 1][column]])//���łɂ��̑�����?�[�`�ɂȂ��Ă��Ȃ�
				{
					counter++;
					attributes[board[row + 1][column]] = true;

				}

				reachCount += counter;
			}
		}

		return reachCount;
	}


	//�]����Ֆʂ�?������?�̈�A��??���s��
	//�Ԃ�l�͕]���l
	inline int evalProcess(BOARD& board)
	{
		int evalNum = 0;
		while (true)
		{
			int combo = evalCombo(board);
			if (combo == 0) break;
			evalNum += combo * 100;//?�[�`��艿�l��?�����邽�߂�100�{

			bool pushed = pushDownDrops(board);
			if (!pushed) break;
		}
		evalNum += countReach(board);

		return evalNum;
	}



	inline int evalProcessCustomized(BOARD& board, std::function<int(BOARD&, UnionFind&)> comboFunction, std::function<int(BOARD&)> reachFunction)
	{
		int evalNum = 0;
		while (true)
		{
			int combo = evalComboCustomized(board, comboFunction);

			bool pushed = pushDownDrops(board);
			if (!pushed) break;
		}
		evalNum += reachFunction(board);

		return evalNum;
	}
}
