using System;

class Program
{
	static int counter;

	static int Main ()
	{
		FormatPrint ($"Case {1}");
		if (counter != 1)
			return 1;

		FormatPrint ($"Case {3}");
		if (counter != 2)
			return 2;

		return 0;
	}

	static void FormatPrint (FormattableString message)
	{
		Console.WriteLine(message);
		++counter;
	}
}
