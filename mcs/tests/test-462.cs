class X {
	bool ok = false;
	
	void Method (X x)
	{
	}

	void Method (string x)
	{
		ok = true;
	}

	public static int Main ()
	{
		X x = new X ();

		x.Method ((string) null);
		if (x.ok)
			return 0;
		return 1;
	}
}
