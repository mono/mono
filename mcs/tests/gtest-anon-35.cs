using System;
using System.Reflection;

// Delegate Cache
class C<T>
{
	static Func<T> XX ()
	{
		System.Func<T> t = () => default (T);
		return t;
	}
}

// Delegate Cache
class C2<T>
{
	static Func<C<T>> XX ()
	{
		System.Func<C<T>> t = () => default (C<T>);
		return t;
	}
}

// No delegate cache
class N1
{
	static Func<T> XX<T> ()
	{
		System.Func<T> t = () => default (T);
		return t;
	}
}

public class Test
{
	public static int Main ()
	{
		var t = typeof (C<>);
		if (t.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length != 1)
			return 1;
		
		t = typeof (C2<>);
		if (t.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length != 1)
			return 1;
		
		t = typeof (N1);
		if (t.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Length != 0)
			return 1;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
