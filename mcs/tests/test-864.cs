class MainClass
{
	static int Foo (double d)
	{
		return 0;
	}

	static int Foo (int d)
	{
		return 100;
	}

	public static int Main ()
	{
		decimal a = new A ();
		long b = new B ();
		if (b != 7)
			return 1;
		
		if (Foo (new B2 ()) != 100)
			return 1;

		return 0;
	}
}

public class A
{
	public static implicit operator int (A a)
	{
		return 6;
	}
}

public class B : A
{
	public static implicit operator int (B b)
	{
		return 7;
	}
}

public class A2
{
	public static implicit operator double (A2 a)
	{
		return 2;
	}
}

public class B2 : A2
{
	public static implicit operator int (B2 b)
	{
		return 3;
	}
}
