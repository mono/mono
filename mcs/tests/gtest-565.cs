using System;

public class B<T>
{
}

public class B2 : B<object>
{
}

public class C
{
	public static void Test<T, I> () where T : B<I>, I
	{
		Foo<T, I> ();
	}

	public static void Foo<T, I> () where T : B<I>, I
	{
	}
	
	public static int Main ()
	{
		Test<B2, object> ();
		return 0;
	}
}

