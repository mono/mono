using System;
using System.Threading.Tasks;

public delegate T ActualValueDelegate<T> ();

class X
{
	public static void Main ()
	{
		Matches (async () => await Throw());
	}

	static bool Matches<T>(ActualValueDelegate<T> del) where T : Task
	{
		del ().Wait ();
		return true;
	}

	static async Task Throw()
	{
		await Task.Delay (1);
	}
}