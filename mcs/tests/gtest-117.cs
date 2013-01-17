// Compiler options: -warnaserror

using System;

enum E { Item };

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
	
	public static int TestC ()
	{
		int? i = null;
		if (i is int) {
			return (int) i;
		}
		
		return 3;
	}
	
	static bool Check1 (E? e)
	{
		return e is Enum;
	}

	static bool Check2<T> (E e) where T : struct
	{
		return e is T;
	}
	
	public static int Main ()
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
		
		if (TestC () != 3)
			return 6;
		
		if (Check1 (null))
			return 7;

		if (!Check1 (E.Item))
			return 8;

		if (Check2<int> (E.Item))
			return 9;
		
		Console.WriteLine ("OK");
		return 0;
	}
}

