using System;

class Base<T> where T : new ()
{
	protected readonly T field = new T ();
}

class Derived<T> : Base<T> where T : ICloneable, new ()
{
	public Derived()
	{
		field.Clone();
	}
}

class C : ICloneable
{
	public object Clone ()
	{
		return null;
	}
	
	public static void Main ()
	{
		var a = new Derived<C> ();
	}
}