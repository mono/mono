using System;

public abstract class A<T>
{
	public abstract G Foo<G> () where G : T;
	
	public virtual G Foo2<G> () where G : T
	{
		return default (G);
	}
}

public class B : A<int?>
{
	public override G Foo<G> ()
	{
		return new G ();
	}
	
	public override G Foo2<G> ()
	{
		return base.Foo2<G> ();
	}
}

abstract class A2<T>
{
	public abstract void Foo<U> () where U : struct, T;
}

class B2 : A2<System.ValueType>
{
	public override void Foo<Y> ()
	{
	}
}

abstract class A3<T>
{
	public abstract void Foo<U> () where U : class, T;
}

class B3 : A3<System.Object>
{
	public override void Foo<Y> ()
	{
	}
}

class Program
{
	public static int Main ()
	{
		var b = new B ();
		if (b.Foo<int?> () == null)
			return 0;
		
		b.Foo2<int?> ();
		
		var b2 = new B2 ();
		b2.Foo<byte> ();
		
		var b3 = new B3 ();
		b3.Foo<string> ();
		
		return 1;
	}
}
