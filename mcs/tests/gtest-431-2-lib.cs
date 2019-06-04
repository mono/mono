// Compiler options: -t:library -noconfig -r:gtest-431-1-lib.dll

using System;

namespace Library {

	public class Foo {
	}

	public static class Extensions {

		public static void Bar (this Foo self)
		{
			Console.WriteLine ("Bar");
		}
	}
}
