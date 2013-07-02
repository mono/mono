using System;

public abstract class CB<T> : CA<T>, IA<T>
{
}

public abstract class CA<T> : IA<T>
{
	public abstract IA<T> Backwards ();

	IB<T> IB<T>.Backwards () { return null; }
}

public interface IA<T> : IB<T>
{
	new IA<T> Backwards ();
}

public interface IB<T>
{
	IB<T> Backwards ();
}

class C
{
	public static void Main ()
	{
	}
}