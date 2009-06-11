// CS1065: Optional parameter is not valid in this context
// Line: 10

delegate void D (int i);

public class C
{
	public static void Main ()
	{
		D d = delegate (int i = 9) { };
	}
}
