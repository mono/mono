using System.Threading.Tasks;
using System;

public class App
{
	X _x = new X ();

	public async Task Test ()
	{
		await Task.Run (new Func<Task> (async () => _x.ToString ()));
	}
}

class X
{
	public static void Main ()
	{
		var app = new App ();
		app.Test ().Wait ();
	}
}
