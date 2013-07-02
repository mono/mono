using System;

public abstract class A<T>
{
	public abstract A<MM> For<MM> () where MM : T;
}

public class B<U, X, V> : A<V>
	where V : X
	where X : U
{
	readonly A<U> _inner;

	public B (A<U> inner)
	{
		_inner = inner;
	}

	public override A<PP> For<PP> () // base constraint is copied as PP : V
	{
		return _inner.For<PP> ();
	}
}

public class Test : A<Test>
{
	public static void Main ()
	{
		var t = new Test ();
		new B<Test, Test, Test> (t).For<Test> ();
	}

	public override A<QQ> For<QQ> ()
	{
		return null;
	}
}
