using System;

public class Misc { //Only append content to this class as the test suite depends on line info
	public static int CreateObject (int foo, int bar) {
		var f = new Fancy () {
			Foo = foo,
			Bar = bar,
		};

		Console.WriteLine ($"{f.Foo} {f.Bar}");
		return f.Foo + f.Bar;
	}
}

public class Fancy {
	public int Foo;
	public int Bar { get ; set; }
}
