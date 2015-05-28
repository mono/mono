using System;
using System.Threading.Tasks;

class Test
{
	public int in_catch, in_finally;

	async Task ExecuteCore ()
	{
		try {
			await Task.Run (() => { 
				throw new ApplicationException ();
			});
		} catch (Exception ex) {
			++in_catch;
			throw;
		} finally {
			++in_finally;
			await Task.Yield ();
		}
	}

	public static int Main ()
	{
		var t = new Test ();
		try {
			t.ExecuteCore ().Wait ();
			return 3;
		} catch (AggregateException) {			
		}

		if (t.in_catch != 1)
			return 1;

		if (t.in_finally != 1)
			return 2;

		return 0;
	}
}