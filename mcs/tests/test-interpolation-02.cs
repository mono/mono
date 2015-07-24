using System;
using System.Linq.Expressions;

class ConversionTest
{
	static int Main ()
	{
		byte b = 3;

		FormattableString c1;
		c1 = $"{b}";
		if (c1.Format != "{0}")
			return 1;

		IFormattable c2;
		c2 = $"format { b }";
		if (!(c2 is FormattableString))
			return 2;

		return 0;
	}
}