// CS0122: `Foo.Bar()' is inaccessible due to its protection level
// Line: 17

using System;

public class Foo
{
	void Bar ()
	{
	}
}

public class Baz : Foo
{
	public static void Main (String[] args)
	{
		Bar ();
	}
}
