using System;
using System.Collections.Generic;

namespace SomeThingTest.Model
{
	public class CloudMessage
	{
		public CloudMessage()
		{
			PositionData = new List<PositionData>();
		}

		public Guid PlayerId { get; set; }
		public List<PositionData> PositionData { get; set; }
		public int Score { get; set; }
	}
}