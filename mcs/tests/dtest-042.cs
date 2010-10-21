class A
{
	protected virtual object Prop { get { return null; } set { } }
}

class B : A
{
	public void Foo ()
	{
	}

	protected override dynamic Prop { get { return new B (); } }
}

class Program : B
{
	void Test ()
	{
		base.Prop.Foo ();
	}

	public static void Main ()
	{
		new Program ().Test ();
	}
}