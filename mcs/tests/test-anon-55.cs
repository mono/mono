using System;

public class Foo
{
	protected delegate void Hello ();

	protected void Test (Hello hello)
	{
		hello ();
	}

	private void Private ()
	{
		Console.WriteLine ("Private!");
	}

	public void Test ()
	{
		Test (delegate {
			Private ();
		});
	}
}

class X
{
	public static void Main ()
	{
		Foo foo = new Foo ();
		foo.Test ();
	}
}
