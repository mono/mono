using x = System;

abstract class Foo<T> : x::IEquatable<T>
{
	public abstract bool Equals (T x);
}

public class C
{
	public static void Main ()
	{
	}
}