using System;

public delegate void D (object o);

public class E<T>
{
	public class I
	{
		public event D E;
	}

	public static void Test ()
	{
		I i = new I ();
		i.E += new D (EH);
	}

	static void EH (object sender)
	{
	}
}

public class M
{
	public static void Main ()
	{
		E<int>.Test ();
	}
}

