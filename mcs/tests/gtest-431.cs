// Compiler options: -r:gtest-431-lib-1.dll -r:gtest-431-lib-2.dll -noconfig

using System;

using Library;

class Program {

	public static void Main ()
	{
		var foo = new Foo ();
		foo.Bar ();
	}
}
