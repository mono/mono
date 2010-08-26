// Dynamic and value types

struct S
{
	public int Value;
}

class C
{
	static dynamic f;
	
	public static int Main ()
	{
		f = new S ();
		f.Value = 5;
		if (f.Value != 5)
			return 1;

		return 0;
	}
}
