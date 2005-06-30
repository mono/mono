// cs0021-2.cs: Cannot apply indexing with [] to an expression of type `Foo'
// Line: 14

using System;
	
public class Foo {
	private int this[int index] { get { return index; } }
}
	
public class Bar {
	public static void Main ()
	{
		Foo foo = new Foo ();
		Console.WriteLine (foo[5]);
	}
}
