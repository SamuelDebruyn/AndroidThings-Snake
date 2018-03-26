using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using SomeThingTest.Model;

namespace SomeThingTest.Services
{
	public class EventHubService : ICloudService
	{
		const string CONNECTION_STRING =
			"Endpoint=sb://things.servicebus.windows.net/;SharedAccessKeyName=send;SharedAccessKey=fEPL8zyGztLWUHozEb2gnvkIw8SOC5EsvodMQqdUI3A=;EntityPath=snakes";

		Guid _currentId;
		EventHubClient _eventHubClient;

		public async Task Connect()
		{
			_eventHubClient = EventHubClient.CreateFromConnectionString(CONNECTION_STRING);
			_currentId = Guid.NewGuid();
		}

		public async Task SendSnakeMove(Board board, int score)
		{
			var message = JsonConvert.SerializeObject(new CloudMessage
			{
				PlayerId = _currentId,
				Score = score,
				PositionData = board.Select(p => new PositionData(p.x, p.y, p.state)).ToList()
			});
			await _eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
		}

		public Task Disconnect()
		{
			return _eventHubClient.CloseAsync();
		}
	}
}