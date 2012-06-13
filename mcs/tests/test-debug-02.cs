class C
{
	public C ()
	{
	}
}

class C1
{
	int a = 55;
}

class C2
{
	int a = 55, b = 33;
	
	public C2 ()
	{
	}
}

class C3
{
	int a = 55;
	
	public C3 ()
		: base ()
	{
	}
}

class C4
{
	public C4 ()
		: this (1)
	{
	}
	
	C4 (int arg)
	{
	}
}

class Test
{
	public static void Main ()
	{
	}
}