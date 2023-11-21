namespace SimpleDatabaseHTTPServer;

using MySql.Data.MySqlClient;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Text;

public class SimpleDatabaseHTTPServer
{
	private HttpListener server;
	private bool isListening;
	private DbConnection dbc;

	public SimpleDatabaseHTTPServer()
	{
		server = new HttpListener();
		server.Prefixes.Add("http://127.0.0.1:8080/");
		isListening = false;
		dbc = new MySqlConnection("server=localhost;port=3306;user=root;password=S3rver_P@$5w0rd;database=popodb;");
	}

	public void StartListening()
	{
		dbc.Open();
		server.Start();
		isListening = true;

		while (isListening)
		{
			try
			{
				HttpListenerContext ctx = server.GetContext();
				Task.Run(() => HandleContext(ctx));
			}
			catch
			{
			}
		}
	}

	public void StopListening()
	{
		isListening = false;
		server.Stop();
		dbc.Close();
	}

	private void HandleContext(HttpListenerContext ctx)
	{
		HttpListenerRequest req = ctx.Request;
		NameValueCollection q = req.QueryString;
		StreamReader sr = new StreamReader(req.InputStream);
		string reqBody = sr.ReadToEnd();
		q.Add(System.Web.HttpUtility.ParseQueryString(reqBody));
		string content = string.Format(@"
		<!DOCTYPE html>
		<html>
		<body>
		<code><pre>{0}</pre></code>
		</body>
		</html>", GetActorQuery(q["name"]));
		byte[] resBody = Encoding.UTF8.GetBytes(content);
		HttpListenerResponse res = ctx.Response;
		res.ContentEncoding = Encoding.UTF8;
		res.ContentLength64 = resBody.Length;
		res.ContentType = "text/html";
		res.StatusCode = (int)HttpStatusCode.OK;
		res.OutputStream.Write(resBody);
		res.Close();
		Console.WriteLine("Done!");
	}

	public string GetActorQuery(string? actorName)
	{
		string query = $@"
		SELECT *
		FROM Movies
		WHERE movieID IN
		(
			SELECT movieID
			FROM MoviesActors
			WHERE actorID =
			(
				SELECT actorID
				FROM Actors
				WHERE name = '{actorName}'
			)
		)
		";

		DbCommand cmd = dbc.CreateCommand();

		cmd.CommandText = query;

		DbDataReader dr = cmd.ExecuteReader();

		string r = "Movie List: ";

		while (dr.Read())
		{
			r += $"{dr.GetString("title")}\n";
		}

		dr.Close();
		cmd.Dispose();

		return r;
	}
}
