using System;
using System.Threading.Tasks;

class MainClass
{
	public static int Main ()
	{
		var t = GetSomeStrings (null);
		try {
			var s = t.Result;
			return 1;
		} catch (AggregateException e) {
			if (e.InnerException is NullReferenceException)
				return 0;

			return 2;
		}
	}

	public static async Task<string> GetSomeStrings (AsyncStringFactory myFactory)
	{
		var res = await myFactory?.GetSomeStringAsync ();
		return res;
	}
}

public class AsyncStringFactory
{
	public async Task<string> GetSomeStringAsync ()
	{
		await Task.Yield();
		return "foo";
	}
}