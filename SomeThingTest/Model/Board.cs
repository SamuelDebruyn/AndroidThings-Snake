using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SomeThingTest.Model
{
	public class Board : IEnumerable<(int x, int y, PositionState state)>
	{
		readonly ReadOnlyDictionary<int, Dictionary<int, PositionState>> _positions;

		public Board(int width, int height)
		{
			var outerDictionary = new Dictionary<int, Dictionary<int, PositionState>>();
			for (var x = 0; x < height; x++)
			{
				outerDictionary[x] = BuildColumnDictionary();
			}

			_positions = new ReadOnlyDictionary<int, Dictionary<int, PositionState>>(outerDictionary);

			Dictionary<int, PositionState> BuildColumnDictionary()
			{
				var inner = new Dictionary<int, PositionState>();
				for (var y = 0; y < width; y++)
				{
					inner[y] = PositionState.Empty;
				}

				return inner;
			}
		}

		public IEnumerator<(int x, int y, PositionState state)> GetEnumerator()
		{
			foreach (var outerPair in _positions)
			{
				foreach (var innerPair in outerPair.Value)
				{
					yield return (outerPair.Key, innerPair.Key, innerPair.Value);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public PositionState GetPositionState(int x, int y)
		{
			return _positions[x][y];
		}

		public void SetPositionState(int x, int y, PositionState postionState)
		{
			_positions[x][y] = postionState;
		}

		public void SetAll(PositionState postionState)
		{
			for (var x = 0; x < _positions.Count; x++)
			{
				for (var y = 0; y < _positions[0].Count; y++)
				{
					SetPositionState(x, y, postionState);
				}
			}
		}

		public List<(int x, int y)> GetEmptyPositions()
		{
			var snakePositions = new List<(int, int)>();

			foreach (var outerPair in _positions)
			{
				foreach (var innerPair in outerPair.Value)
				{
					if (innerPair.Value == PositionState.Empty)
					{
						snakePositions.Add((outerPair.Key, innerPair.Key));
					}
				}
			}

			return snakePositions;
		}
	}
}