using System;

class C
{
	public static void Main ()
	{
		G<C>.Foo ();
		G2<C>.Foo ();
	}
}

class G<T> where T : new ()
{
	public static void Foo ()
	{
		Console.WriteLine ((new T ()).GetType ().ToString ());
	}
}

class G2<T> where T : class, new ()
{
	public static void Foo ()
	{
		Console.WriteLine ((new T ()).GetType ().ToString ());
	}
}
