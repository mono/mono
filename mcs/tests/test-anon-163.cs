using System;

class A
{
	public virtual void Foo<T> (T a, params string[] b) where T : struct
	{
	}

	protected virtual void Foo2<T> ()
	{
	}

	public virtual T Foo4<T> ()
	{
		return default (T);
	}
}

class B : A
{
	public void Test (int v)
	{
		Action a = () => base.Foo<int> (b: "n", a: v);
		a ();
	}

	public void Test2<T> (T b) where T : struct
	{
		Action a2 = () => base.Foo<T> (b, "as", "asdfa");
	}

	internal void Test3 ()
	{
		int i = 0;
		Action a = delegate () {
			i = 1;
			base.Foo2<string> ();
		};

		a ();

		a = delegate () {
			i = 2;
			base.Foo2<int> ();
		};

		a ();
	}

	public T Test4<T> ()
	{
		Func<T> a4 = () => base.Foo4<T> ();
		return a4 ();
	}
}

class Test
{
	public static void Main ()
	{
		var b = new B ();
		b.Test (1);
		b.Test2 (2);
		b.Test3 ();
	}
}