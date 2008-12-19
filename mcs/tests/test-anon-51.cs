using System;

public delegate void FooDelegate ();

public class X {
	public static readonly FooDelegate Print = delegate {
		Console.WriteLine ("delegate!");
        };

	public static void Main ()
	{
		Print ();
	}
}
