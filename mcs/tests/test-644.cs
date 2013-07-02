using System;

class Program
{
	public delegate object D (int member);

	private D _value;

	private object M (int member)
	{
		return null;
	}

	void Test_1 ()
	{
		Delegate d1 = M + _value;
		Delegate d2 = _value + M;
	}

	public bool Test_2 ()
	{
		return _value == M;
	}

	public bool Test_3 ()
	{
		return _value != M;
	}
	
	public bool Test_4 (D d)
	{
		return d == _value;
	}
	
	public static int Main ()
	{
		Program p = new Program ();
		if (p.Test_2 ())
			return 1;
		p._value = p.M;
		if (!p.Test_2 ())
			return 2;
		
		if (p.Test_3 ())
			return 3;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
