using System;
using System.Linq.Expressions;

namespace System
{
	public class FormattableString
	{
		public FormattableString (string str, object[] arguments)
		{
			Value = str;
			Arguments = arguments;
		}

		public string Value { get; set; }
		public object[] Arguments;
	}
}

namespace System.Runtime.CompilerServices
{
	public static class FormattableStringFactory
	{
		public static object Create(string format, params object[] arguments)
		{
			if (format.StartsWith ("format"))
				return new MyFormattable ();

			return new FormattableString (format, arguments);
		}
	}
}

class MyFormattable : IFormattable
{
	string IFormattable.ToString (string str, IFormatProvider provider)
	{
		return null;
	}
}

class ConversionTest
{
	static int Main ()
	{
		byte b = 3;

		FormattableString c1;
		c1 = $"{b}";
		if (c1.Value != "{0}")
			return 1;

		IFormattable c2;
		c2 = $"format { b }";
		if (!(c2 is MyFormattable))
			return 2;

		return 0;
	}
}