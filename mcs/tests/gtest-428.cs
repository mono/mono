using System;

public struct CInt
{
	int data;

	public CInt (int data)
	{
		this.data = data;
	}

	public static implicit operator CInt (int xx)
	{
		return new CInt (xx);
	}

	public static implicit operator int (CInt xx)
	{
		return xx.data;
	}
}


public class Klass
{
	CInt? t;
	public Klass (CInt? t)
	{
		this.t = t;
	}

	public CInt? Value
	{
		get
		{
			return t;
		}
	}
}

public class MainClass
{
	public static int Main ()
	{
		var v = new Klass (new CInt (3));

		if (v.Value == 1)
			return 1;

		if (v.Value != 3)
			return 2;

		if (v.Value == null)
			return 3;

		var v2 = new Klass (null);

		if (v2.Value != null)
			return 4;

		Console.WriteLine ("OK");
		return 0;
	}
}
