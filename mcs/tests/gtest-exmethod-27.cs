using System;
using System.Collections.Generic;

namespace Foo
{

	static partial class Extensions
	{
	}

	static partial class Extensions
	{

		public static string AsString (this IList<byte> bytes)
		{
			return "42";
		}
	}
}

namespace Bar
{

	using Foo;

	class Program
	{

		public static void Main ()
		{
			Console.WriteLine (Pan (new byte[0]));
		}

		internal static string Pan (byte[] bytes)
		{
			return bytes.AsString ();
		}
	}
}
