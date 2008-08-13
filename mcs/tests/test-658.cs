class A
{
	private int Thread
	{
		get
		{
			return 0;
		}
	}
}

class Thread
{
	public static void Foo ()
	{
	}
}

class B : A
{
	public static void Main ()
	{
		Thread.Foo ();
	}
}

