using System;

class Model
{
	public int Value;
}

class C1<T1>
{
	public void Add (Func<T1, int> t)
	{
	}
}

abstract class C2<TModel>
{
	public abstract void ApplyImpl<U> (C1<U> c1) where U : TModel;
}

class C3 : C2<Model>
{
	public override void ApplyImpl<Foo> (C1<Foo> c1)
	{
		c1.Add (t => t.Value);
	}
}

class Program
{
	static void Main ()
	{
		var v1 = new C1<Model> ();
		var c3 = new C3 ();
		c3.ApplyImpl (v1);
	}
}
