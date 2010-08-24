struct S
{
}

class C
{
	public static int Main ()
	{
		dynamic d = new S ();
		bool b = d is S;
		if (!b)
			return 1;
		
		return 0;
	}
}
