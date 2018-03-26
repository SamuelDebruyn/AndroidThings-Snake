using System.Threading.Tasks;
using SomeThingTest.Model;

namespace SomeThingTest.Services
{
	public interface ICloudService
	{
		Task SendSnakeMove(Board board, int score);
		Task Disconnect();
		Task Connect();
	}
}