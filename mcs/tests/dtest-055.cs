using System;

struct S<T>
{
}

abstract class B<T1, T2>
{
	public abstract void Foo<U> (U x) where U : T1, T2;
}

class C : B<S<object>, S<dynamic>>
{
	public override void Foo<U> (U x)
	{
	}

	public static int Main ()
	{
		var m = typeof (C).GetMethod ("Foo");
		var ta = m.GetGenericArguments ()[0].GetGenericParameterConstraints ();
		if (ta.Length != 1)
			return 1;
		
		Console.WriteLine ("ok");
		return 0;
	}
}