using System;
using System.Threading.Tasks;

class X
{
	static Exception ex = new ApplicationException ();

	public static int Main ()
	{
		if (Test (5, null).Result != 5)
			return 1;

		try {
			Test (5, ex).Wait ();
			return 2;
		} catch (AggregateException ae) {
			if (ae.InnerException != ex)
				return 3;
		}

		try {
			Test (15, ex).Wait ();
			return 4;
		} catch (AggregateException ae) {
			if (ae.InnerException != ex)
				return 5;
		}

		try {
			TestGeneric (5).Wait ();
			return 10;
		} catch (AggregateException ae) {
			if (ae.InnerException != ex)
				return 11;
		}
		
		try {
			TestGeneric (15).Wait ();
			return 12;
		} catch (AggregateException ae) {
			if (ae.InnerException != ex)
				return 13;
		}

		return 0;
	}

	async static Task<int> Test (int x, Exception e)
	{
		try {
			Console.WriteLine (x);
			if (e != null)
				throw e;
		} catch (Exception) if (x != 15) {
			await Task.FromResult (0);
			throw;
		}

		return x;
	}

	async static Task<int> TestGeneric (int x)
	{
		try {
			Console.WriteLine (x);
			throw ex;
		} catch if (x != 15) {
			await Task.FromResult (0);
			throw;
		}

		return x;
	}
}