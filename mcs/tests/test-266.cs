using System;

enum Foo { Bar }
class T {
	public static int Main ()
	{
		Enum e = Foo.Bar;
		IConvertible convertible = (IConvertible) e;
		IComparable comparable = (IComparable) e;
		IFormattable formattable = (IFormattable) e;
		
		Console.WriteLine ("PASS");
		return 0;
	}
}

