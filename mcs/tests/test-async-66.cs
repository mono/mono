using System;
using System.Threading.Tasks;

class TestFinally
{
	static int counter;

	async static Task Test (bool throwException)
	{
		try {
			if (throwException)
				throw new ApplicationException ();

			++counter;
			System.Console.WriteLine ();
		} finally {
			counter += 10;
			await Task.Delay (2);
			counter += 100;
		}
		counter += 1000;
	}

	static int Main ()
	{
		Test (false).Wait ();
		if (counter != 1111)
			return 1;

		counter = 0;
		try {
			Test (true).Wait ();
			return 2;
		} catch (AggregateException) {			
		}

		if (counter != 110)
			return 3;

		return 0;
	}
}