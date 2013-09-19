using System.Threading.Tasks;
using System;

class X
{
	static void Main ()
	{
		var x = new X ();
		x.Run ().Wait ();
	}

	Task<int> AnimateAsync (Action callback)
	{
		callback ();
		return Task.FromResult (2);
	}

	void SecondLevel (Action callback)
	{
		callback ();
	}

	async Task Run ()
	{
		var ret = await AnimateAsync (() => {
			SecondLevel (() => {
				Console.WriteLine (this);
			});
		});
	}
}
