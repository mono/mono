using System;
using System.Threading.Tasks;

class X
{
	public static int Main ()
	{
		if (new X ().Test (false).Result != true)
			return 1;

		if (new X ().Test (true).Result != true)
			return 2;

		return 0;
	}

	public async Task<bool> Test(bool TrueOrFalse)
	{
		if (TrueOrFalse)
			return true;

		try {
			return true;
 		}
		finally
		{
			await Task.Yield ();
		}
	}
}