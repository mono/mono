// CS0121: The call is ambiguous between the following methods or properties: `A.GetValues(string[], string)' and `A.GetValues(string, params string[])'
// Line: 23
// CSC BUG: Correct according the spec, no identity conversion to do tie-breaking

class A
{
	public int GetValues (string[] s, string value = null)
	{
		return 1;
	}

	public int GetValues (string s, params string [] args)
	{
		return 2;
	}
}


class B
{
	public static void Main ()
	{
		var a = new A ();
		a.GetValues (null);
	}
}