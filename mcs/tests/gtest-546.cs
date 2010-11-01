using System;

class Factory
{
	public class S<G1, G2>
	{
	}
	
	public static S<F1, F2> Create<F1, F2> (F1 f1, F2 f2)
	{
		return null;
	}
}

class A
{
	static TR Test<T1, T2, TR>(T1 t1, T2 t2, Func<T1, T2, TR> f)
	{
		return f (t1, t2);
	}
	
	static void Main ()
	{
		var r = Test ("a", "b", Factory.Create);
	}
}