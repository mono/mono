using System;

public interface A
{
	void Foo ();
}

public interface B : A
{ }

public abstract class X : A
{
	public abstract void Foo ();
}

public abstract class Y : X, B
{ }

public class Z : Y
{
	public override void Foo ()
	{
		Console.WriteLine ("Hello World!");
	}
}

class Test
{
	public static int Main ()
	{
		Z z = new Z ();
		A a = z;
		a.Foo ();
		return 0;
	}
}
