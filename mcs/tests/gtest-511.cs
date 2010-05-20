interface IA : IB
{
}

interface IB
{
}

class A<T> where T : IA
{
}

class C
{
	public virtual void Foo<T> (A<T> t) where T : IA
	{
	}
}

class D : C
{
	public override void Foo<T> (A<T> t)
	{
	}
	
	public static int Main ()
	{
		new D ();
		
		var m = typeof (D).GetMethod ("Foo");
		var ga = m.GetGenericArguments() [0];
		
		var tpConstraints = ga.GetGenericParameterConstraints();
		if (tpConstraints.Length != 1)
			return 1;
		
		if (tpConstraints [0] != typeof (IA))
			return 2;
		
		return 0;
	}
}