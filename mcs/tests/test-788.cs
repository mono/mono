using System;

class Program
{
	static int Main ()
	{
		B b = new B ();
		if (b.Message != "OK")
			return 1;
		return 0;
	}
}

class A
{
	public virtual string Message
	{
		get
		{
			return "OK";
		}
	}
}

class B : A
{
	new string Message
	{
		get
		{
			throw new Exception ();
		}
	}
}

