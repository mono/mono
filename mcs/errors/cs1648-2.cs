// CS1648: Members of readonly field `Test.p' cannot be modified (except in a constructor or a variable initializer)
// Line: 17

using System;

public class Test
{
	struct Container
	{
		public int foo { get; set; }
	}
	
	readonly Container p;
	
	void Foo ()
	{
		p.foo = 0;
	}

	public static void Main ()
	{
	}
}