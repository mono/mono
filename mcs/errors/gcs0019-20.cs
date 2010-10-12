// CS0019: Operator `>' cannot be applied to operands of type `S' and `S?'
// Line: 9

public class Test
{
	public static void Main ()
	{
		S a = new S ();
		S? b = null;
		string res = a > b;
	}
}

struct S
{
	public static string operator > (S a, S b)
	{ 
		return ">";
	}
	
	public static string operator < (S a, S b)
	{ 
		return "<";
	}
}
