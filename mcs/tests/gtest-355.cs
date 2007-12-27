class A
{
	public virtual bool Foo (string s)
	{
		return true;
	}

	public virtual string Foo<T> (string s)
	{
		return "v";
	}
}

class B : A
{
	public bool Goo (string s)
	{
		return Foo (s);
	}

	public override bool Foo (string s)
	{
		return false;
	}

	public override string Foo<T> (string s)
	{
		return "a";
	}
}


class C
{
	public static void Main ()
	{
	}
}