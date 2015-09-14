// CS0023: The `?' operator cannot be applied to operand of type `method group'
// Line: 14

class X
{
	void Test ()
	{
	}

	public static void Main ()
	{
		X x = null;
		System.Action n = x?.Test;
	}
}