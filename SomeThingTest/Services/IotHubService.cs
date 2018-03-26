//using System;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Azure.Devices.Client;
//using ThingsPlayground.Model;
//
//namespace ThingsPlayground.Services
//{
//	public class IotHubService : ICloudService
//	{
//		const string CONNECTION_STRING = "HostName=snakehub.azure-devices.net;SharedAccessKeyName=device;SharedAccessKey=rN6b/xpOBv2Oiirf4UAZ6LCgL+fidkphKzGZQrcZj78=";
//		Guid _currentId;
//		DeviceClient _deviceClient;
//
//		public async Task Connect()
//		{
//			_currentId = Guid.NewGuid();
//			_deviceClient = DeviceClient.CreateFromConnectionString(CONNECTION_STRING);
//			await _deviceClient.OpenAsync();
//		}
//
//		public Task SendSnakeMove(Board board, int score)
//		{
//			var message = Newtonsoft.Json.JsonConvert.SerializeObject(new CloudMessage
//			{
//				PlayerId = _currentId,
//				Score = score,
//				PositionData = board.Select(p => new PositionData(p.x, p.y, p.state)).ToList()
//			});
//			return _deviceClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes(message)));
//		}
//
//		public Task Disconnect()
//		{
//			return _deviceClient.CloseAsync();
//		}
//	}
//}

