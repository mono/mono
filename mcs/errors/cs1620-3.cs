// CS1620: Argument `#1' is missing `out' modifier
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

