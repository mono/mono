// CS8093: An argument to nameof operator cannot be extension method group
// Line: 35

using System;

public class A
{
}

namespace N1
{
	public static class X
	{
		public static string Extension (this A a, long x)
		{
			return null;
		}
		
		public static string Extension (this A a, int x)
		{
			return null;
		}
	}
}

namespace N2
{
	using N1;
	
	public class Program
	{
		public static int Main ()
		{
			A a = null;
			const string n = nameof (a.Extension);
			if (n != "Extension")
				return 1;

			return 0;
		}
	}
}