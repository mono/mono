// CS0165: Use of unassigned local variable `g'
// Line: 10

public class A
{
	static bool Test7 ()
	{
		int f = 1;
		int g;
		return f > 1 && OutCall (out g) || g > 1;
	}

	static bool OutCall (out int arg)
	{
		arg = 1;
		return false;
	}
}