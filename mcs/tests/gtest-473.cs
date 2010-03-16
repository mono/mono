class A<X>
{
	public virtual void Foo<T> () where T : A<T>
	{
	}
}

class B : A<int>
{
	public override void Foo<T> ()
	{
	}
}

class C
{
	public static int Main ()
	{
		new B ();
		return 0;
	}
}