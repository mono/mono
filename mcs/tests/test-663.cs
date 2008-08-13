class A
{
	public static implicit operator int (A a)
	{
		return 1;
	}
	
	public static implicit operator bool (A a)
	{
		return false;
	}
}

class C
{
	public static void Main ()
	{
		switch (new A ())
		{
			default: break;
		}
	}
}