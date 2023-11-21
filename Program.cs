namespace SimpleDatabaseHTTPServer;

public class Program
{
	public static async Task Main(string[] args)
	{
		SimpleDatabaseHTTPServer server = new SimpleDatabaseHTTPServer();

		Task serverListenTask = Task.Run(() => server.StartListening());
		//await Task.Delay(5000);
		//server.StopListening();
		await serverListenTask;
	}
}
