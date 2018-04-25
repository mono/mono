// CS8150: By-reference return is required when method returns by reference
// Line: 10

class A
{
	int p;

	ref int Test ()
	{
		return p;
	}
}