// CS1620: Argument `1' must be passed with the `out' keyword
// Line: 24

using System;

namespace TestNamespace
{
	public class Test
	{
		public Test ()
		{
			int os;
			TestMethod (os);
			Console.WriteLine (os);
		}

		public void TestMethod (out int os)
		{
			os = 3;
		}
	}
}

