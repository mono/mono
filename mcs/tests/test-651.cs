using System.Collections;

class Foo
{
	void Open (IList a)
	{
	}

	void Open (out ArrayList a)
	{
		a = null;
		Open ((IList) a);
		Open (a);
	}
	
	public static void Main ()
	{
	}
}
