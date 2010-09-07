using System;

class A
{
	public virtual void Foo<T> (T a, params string[] b) where T : struct
	{
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
}

class Test
{
	public static void Main ()
	{
		new B ().Test (1);
	}
}