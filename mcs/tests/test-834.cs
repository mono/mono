using System;

class A
{
	public int Value;
	
	public A (object o)
	{
		Value = 500;
	}

	protected A (int a)
	{
		Value = a;
	}
	
	public int Test (object o)
	{
		return 2;
	}
	
	protected int Test(int i)
	{
		return 5;
	}
	
	protected int this [int i] {
		get { return i; }
	}
	
	public int this [object i] {
		get {
			return 2;
		}
	}
}

class B : A
{
	public B ()
		: base (1)
	{
	}
	
	public static int Main ()
	{
		int r;
		A a = new A (1);
		if (a.Value != 500)
			return 1;
		
		r = a.Test (1);
		if (r != 2)
			return 2;
		
		r = a [0];
		if (r != 2)
			return 3;
		
		Console.WriteLine ("ok");
		return 0;
	}
}