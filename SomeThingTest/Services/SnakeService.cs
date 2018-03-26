using System;
using System.Collections.Generic;
using System.Linq;
using SomeThingTest.Model;

namespace SomeThingTest.Services
{
	public class SnakeService
	{
		readonly LinkedList<(int x, int y)> _currentSnake;
		readonly int _height;
		readonly Random _randomizer;
		readonly int _width;

		Direction _lastDirection;
		Direction _nextDirection;

		public SnakeService(int width, int height)
		{
			_width = width;
			_height = height;
			_randomizer = new Random();
			Board = new Board(width, height);
			_currentSnake = new LinkedList<(int x, int y)>();

			Start();
		}

		public Board Board { get; }
		public int CurrentScore { get; private set; }
		public bool GameEnded { get; private set; }

		public void Start()
		{
			GameEnded = false;
			_currentSnake.Clear();
			CurrentScore = 0;

			_lastDirection = _nextDirection = (Direction) _randomizer.Next(0, Enum.GetValues(typeof(Direction)).Cast<int>().Max());
			Board.SetAll(PositionState.Empty);

			(int x, int y) startPosition = (_randomizer.Next(0, _height), _randomizer.Next(0, _width));
			_currentSnake.AddFirst(startPosition);
			Board.SetPositionState(startPosition.x, startPosition.y, PositionState.Snake);

			AddNewFood();
		}

		public void Turn(Direction direction)
		{
			if (_lastDirection == direction || AreOpposites(_lastDirection, direction))
			{
				return;
			}

			_nextDirection = direction;
		}

		public bool Advance()
		{
			var head = _currentSnake.First.Value;
			var tail = _currentSnake.Last.Value;

			var snakeGrows = false;

			(int x, int y) nextPosition = (GetNextX(head.x), GetNextY(head.y));
			var currentStateOnNextPosition = Board.GetPositionState(nextPosition.x, nextPosition.y);

			switch (currentStateOnNextPosition)
			{
				case PositionState.Snake:
					GameEnded = true;
					return false;
				case PositionState.Food:
					snakeGrows = true;
					break;
			}

			Board.SetPositionState(nextPosition.x, nextPosition.y, PositionState.Snake);
			_currentSnake.AddFirst(nextPosition);

			if (!snakeGrows)
			{
				Board.SetPositionState(tail.x, tail.y, PositionState.Empty);
				_currentSnake.RemoveLast();
			}
			else
			{
				var addedNewFood = AddNewFood();
				if (!addedNewFood)
				{
					GameEnded = true;
					return false;
				}

				CurrentScore++;
			}

			_lastDirection = _nextDirection;
			return true;
		}

		bool AddNewFood()
		{
			var emptyPositions = Board.GetEmptyPositions();

			if (emptyPositions.Count == 0)
			{
				return false;
			}

			var newFoodPosition = emptyPositions[_randomizer.Next(0, emptyPositions.Count - 1)];
			Board.SetPositionState(newFoodPosition.x, newFoodPosition.y, PositionState.Food);

			return true;
		}

		int GetNextX(int currentX)
		{
			switch (_nextDirection)
			{
				case Direction.Left:
					return currentX == 0 ? _height - 1 : currentX - 1;
				case Direction.Right:
					return currentX == _height - 1 ? 0 : currentX + 1;
				default:
					return currentX;
			}
		}

		int GetNextY(int currentY)
		{
			switch (_nextDirection)
			{
				case Direction.Up:
					return currentY == 0 ? _height - 1 : currentY - 1;
				case Direction.Down:
					return currentY == _height - 1 ? 0 : currentY + 1;
				default:
					return currentY;
			}
		}

		static bool AreOpposites(Direction first, Direction second)
		{
			switch (first)
			{
				case Direction.Up:
				case Direction.Down:
					return second == Direction.Up || second == Direction.Down;
				default:
					return second == Direction.Left || second == Direction.Right;
			}
		}
	}
}