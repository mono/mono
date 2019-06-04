// CS1605: Cannot pass `this' as a ref or out argument because it is read-only
// Line: 14
// Compiler options: -langversion:latest

readonly struct X
{
	void Test (out X x)
	{
		x = new X ();
	}
	
	void Run ()
	{
		Test (out this);
	}
}
