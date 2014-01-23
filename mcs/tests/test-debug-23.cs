// Tests for explicit sequence point on non-empty stack

using System;

class Program
{
	void Test_1 ()
	{
		Prop.BindCore();
	}
	
	void Test_2 ()
	{
		Prog ().BindCore();
	}
	
	void Test_3 ()
	{
		new Program ().BindCore ();
	}
	
	void Test_4 ()
	{
		Func<Program> f = () => new Program ();
		f ().BindCore ();
	}

	public int BindCore ()
	{
		return 3;
	}

	public Program Prog () 
	{
		return this;
	}

	public Program Prop {
		get {
			return this;
		}
	}

	public static void Main ()
	{
	}
}