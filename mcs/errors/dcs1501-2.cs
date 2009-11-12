// CS1501: No overload for method `this' takes `1' arguments
// Line: 13

public class Blah
{
	int this [short id, string v] {
		set {}
	}

	public void Test ()
	{
		dynamic d = 1;
		this [d] = 1;
	}
}
