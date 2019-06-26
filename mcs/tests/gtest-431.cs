// Compiler options: -r:gtest-431-1-lib.dll -r:gtest-431-2-lib.dll -noconfig

using System;

using Library;

class Program {

	public static void Main ()
	{
		var foo = new Foo ();
		foo.Bar ();
	}
}
