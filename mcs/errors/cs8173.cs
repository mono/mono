// CS8173: The expression must be of type `long' because it is being assigned by reference
// Line: 11

public class X
{
	int field;

	public static void Main ()
	{
		int i = 5;
		ref long j = ref i;
	}
}