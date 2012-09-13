using System;

public class A<T>
{
}

class Program
{
	public static void Foo<TEventArgs, TEventHandler> (A<TEventHandler> info, Action<object, TEventArgs> action)
	{
	}

	static void Main ()
	{
		A<string> pp = null;
		Foo (pp, (object s, string e) => { });
	}
}
