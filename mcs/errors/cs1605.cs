// CS1605: Cannot pass `this' as a ref or out argument because it is read-only
// Line: 13

class X
{
	void Test (out X x)
	{
		x = null;
	}
	
	void Run ()
	{
		Test (out this);
	}
}
