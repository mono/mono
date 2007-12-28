// CS1502: The best overloaded method match for `X.M(string)' has some invalid arguments
// Line: 23

delegate void D1 (object o);
delegate void D2 ();

class C
{
	public C (D1 d)
	{
	}
	
	public C (D2 d)
	{
	}
}

class X
{
	void Foo ()
	{
		new C (delegate (object state) {
			M (1);
		});
	}

	void M (string s)
	{
	}
}

