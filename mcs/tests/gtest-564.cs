using System;

struct S
{
}

abstract class B1<T1, T2>
{
	public abstract void Test<U> (U x) where U : struct, T1, T2;
}

abstract class B2<T1, T2>
{
	public abstract void Test<U> (U x) where U : class, T1, T2;
}

class C1 : B1<S, ValueType>
{
	public override void Test<U> (U x)
	{
	}
}

class C2 : B2<string, object>
{
	public override void Test<U> (U x)
	{
	}
}

class Test
{
	public static int Main ()
	{
		var m = typeof (C1).GetMethod ("Test");
		var ta = m.GetGenericArguments ()[0].GetGenericParameterConstraints ();
		if (ta.Length != 2)
			return 1;
		
		m = typeof (C2).GetMethod ("Test");
		ta = m.GetGenericArguments ()[0].GetGenericParameterConstraints ();
		if (ta.Length != 1)
			return 2;

		Console.WriteLine ("ok");
		return 0;
	}
}
