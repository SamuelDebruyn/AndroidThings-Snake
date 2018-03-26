namespace SomeThingTest.Model
{
	public struct PositionData
	{
		public int X, Y;
		public PositionState State;

		public PositionData(int x, int y, PositionState state)
		{
			X = x;
			Y = y;
			State = state;
		}
	}
}