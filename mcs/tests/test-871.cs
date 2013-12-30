using System;

class D
{
	int arg;

	public D (int arg)
	{
		this.arg = arg;
	}

	public static D operator & (D x, D y)
	{
		return new D (100);
	}

	public static bool operator false (D d)
	{
		return false;
	}

	public static bool operator true (D d)
	{
		return true;
	}

	public static implicit operator D(bool b)
	{
		return new D (5);
	}

	static int Main ()
	{
		D d = false && new D (1);
		Console.WriteLine (d.arg);
		if (d.arg != 100)
			return 1;

		Console.WriteLine ("ok");
		return 0;
	}
}