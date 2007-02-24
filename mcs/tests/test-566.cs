public class Test
{
	private C _vssItem;

	public string Spec
	{
		get { return _vssItem.Spec; }
	}
	
	void Foo (C c)
	{
		c.Checkout ();
	}
	
	void Foo2 (CC cc)
	{
		cc.Spec = "aa";
	}

	public static void Main ()
	{
	}
}

interface A
{
	void Checkout ();
	string Spec
	{
		get;
	}
}

interface B : A
{
	new void Checkout ();
	new string Spec
	{
		get;
	}
}

interface C : B
{
}

class CA
{
	public string Spec
	{
		set {}
	}
}

class CB : CA
{
	new public string Spec
	{
		set {}
	}
}

class CC : CB
{
}