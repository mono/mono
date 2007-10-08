// CS0122: `Foo.this[int]' is inaccessible due to its protection level
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
