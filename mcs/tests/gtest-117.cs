// Compiler options: -warnaserror

using System;

public interface IFoo<T>
{ }

public class Foo<T>
{
	public static bool Test (T x)
	{
		return x is IFoo<T>;
	}
	
	public static bool Test ()
	{
		T t = default (T);
		return t is int;
	}
	
	public static bool TestB ()
	{
		T t = default (T);
		return t is int?;
	}
}

class Y<T> where T : struct
{
	public bool Foo ()
	{
		object o = null;
		return o is System.Nullable <T>;
	}
}

class X
{
	public static bool TestA (object o)
	{
		return o is int?;
	}
	
	public static bool TestB<T> (T o)
	{
		return o is int[];
	}
	
	static int Main ()
	{
		if (Foo<int>.Test (3))
			return 1;
		
		if (!Foo<int>.Test())
			return 2;
		
		// False expected int? != null
		if (Foo<int?>.TestB())
			return 3;

		int? i = 0;
		if (!TestA(i))
			return 4;
		
		int[] a = new int[0];
		if (!TestB(a))
			return 5;
		
		Console.WriteLine ("OK");
		return 0;
	}
}

