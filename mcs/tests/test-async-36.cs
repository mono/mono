using System;
using System.Threading.Tasks;

class X
{
	internal Task<int> ExecuteInternalAsync ()
	{
		return Task.FromResult (1);
	}

	public async Task<object> ExecuteReaderAsync ()
	{
		await ExecuteInternalAsync ();
		return Task.FromResult (0);
	}

	public static int Main ()
	{
		var at = new X ().ExecuteReaderAsync ();
		if (!at.Wait (1000))
			return 1;

		return 0;
	}
}
