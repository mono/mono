// CS8160: A readonly field cannot be returned by reference
// Line: 10

class X
{
	readonly int f = 0;

	ref int Test ()
	{
		return ref f;
	}
}