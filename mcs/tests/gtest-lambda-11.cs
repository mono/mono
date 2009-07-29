

using System;

public class Pair<T1, T2>
{
}

public delegate Pair<T1, T2> Group<T1, T2>(T1 input);


public static class C
{
	public static void Foo<TInput, TValue, TIntermediate> (Group<TInput, TValue> Pair,
		Func<TValue, Group<TInput, TIntermediate>> selector)
	{
	}
}

public class E<TI>
{
	public void Rep1<TV>(Group<TI, TV> parser)
	{
		C.Foo (parser, (x) => parser);
	}
}

public class M
{
	public static void Main ()
	{
	}
}

