// CS8161: A static readonly field cannot be returned by reference
// Line: 10

class X
{
	static readonly int f;

	static ref int Test ()
	{
		return ref f;
	}
}