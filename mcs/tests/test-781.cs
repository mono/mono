using System;

delegate void D ();

class E
{
	public event D temp;
}

class A
{
	E Prop	{
		get {
			return new E ();
		}
	}

	void Test ()
	{
		Prop.temp += delegate () { };
	}

	public static void Main ()
	{
		var a = new A ();
		a.Test ();
	}
}
