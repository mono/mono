using System;
using System.Threading.Tasks;

public class Test
{
	static async Task<string> AsyncWithDeepTry ()
	{
		try {
			await Task.Yield ();

			try {
				await Task.Yield ();
			} catch {
			}
		} catch {
			await Task.Yield ();
		} finally {
		}

		return null;
	}


	static void Main ()
	{
		AsyncWithDeepTry ().Wait ();
	}
}
