using System;
using System.Threading.Tasks;

class Program
{
	static int count;

	static int Main ()
	{
		Test (false).Wait ();
		Console.WriteLine (count);
		if (count != 110011)
			return 1;

		count = 0;
		Test (true).Wait ();
		Console.WriteLine (count);
		if (count != 111101)
			return 2;

		count = 0;
		Test2 (false).Wait ();
		Console.WriteLine (count);
		if (count != 11)
			return 3;

		count = 0;
		Test2 (true).Wait ();
		Console.WriteLine (count);
		if (count != 1101)
			return 4;

		return 0;
	}

	static async Task Test (bool throwTest)
	{
		try {
			count += 1;
			await Task.Delay (10);

			if (throwTest)
				throw new ApplicationException ();

			count += 10;
		} catch (ApplicationException) {
			count += 100;
		   	await Task.Delay (10);
			count += 1000;
		} finally {
			count += 10000;
			await Task.Delay (10);
			count += 100000;
		}
	}

	static async Task Test2 (bool throwTest)
	{
		try {
			count += 1;
			await Task.Delay (10);

			if (throwTest)
				throw new ApplicationException ();

			count += 10;
		} catch (ApplicationException) {
			count += 100;
		   	await Task.Delay (10);
			count += 1000;
		} finally {
		}
	}	
}
