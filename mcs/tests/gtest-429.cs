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
	public CInt? Value;
	public Klass (CInt? t)
	{
		this.Value = t;
	}
}

public class MainClass
{
	public static int Main ()
	{
		var v = new Klass (3);
		return v.Value.Value - 3;
	}
}
