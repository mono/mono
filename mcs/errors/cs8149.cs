// CS8149: By-reference returns can only be used in methods that return by reference
// Line: 10

class A
{
	int p;

	int Test ()
	{
		return ref p;
	}
}