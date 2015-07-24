using System;
using System.Threading.Tasks;

class Test
{
	static bool Verify (Func<bool> f)
	{
		return f ();
	}

	static async Task<int> TestCapturedException (Exception e)
	{
		try {
			if (e != null)
				throw e;
		} catch (Exception ex) when (Verify (() => ex.Message == "foo")) {
			await Task.Yield ();
			Console.WriteLine (ex);
			return 1;
		} catch (Exception ex) when (Verify (() => ex.Message != null)) {
			await Task.Yield ();
			Console.WriteLine (ex);
			return 2;
		}

		return 3;
	}

	public static int Main()
	{
		if (TestCapturedException (null).Result != 3)
			return 1;

		var ex = new ApplicationException ();
		if (TestCapturedException (ex).Result != 2)
			return 2;

		return 0;
	}
}