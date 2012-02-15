using System;

class C
{
	event Func<int> OutEvent;
	
	public static void Main ()
	{
	}
	
	public void M<U, V> (out U u, ref V v)
	{
		u = default (U);
	}
	
	void Test_1 ()
	{
		dynamic d = "a";
		string s = d + "b";
		return;
	}
	
	void Test_2 ()
	{
		dynamic d = "a";
		d.ToString ();
	}
	
	void Test_3 ()
	{
		dynamic u = "s";
		dynamic v = 5;
		dynamic c = new C ();
		c.M (out u, ref v);
	}
	
	void Test_4 ()
	{
		dynamic d = new C ();
		d.OutEvent += new Func<int> (() => 100);
	}
}
