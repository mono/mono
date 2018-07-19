// CS8151: The return by reference expression must be of type `string' because this method returns by reference
// Line: 10

public class X
{
	int field;

	ref string TestMethod ()
	{
		return ref field;
	}
}
