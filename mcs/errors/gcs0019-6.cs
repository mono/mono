// CS0019: Operator `==' cannot be applied to operands of type `method group' and `method group'
// Line: 8

public class C
{
	public static void Main ()
	{
		bool a = DelegateMethod == DelegateMethod;
	}

	static int DelegateMethod(bool b)
	{
		return 3;
	}	
}
