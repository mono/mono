// CS1604: Cannot assign to `this' because it is read-only
// Line: 8
// Compiler options: -langversion:latest

readonly struct S
{
	void Foo ()
	{
		this = new S ();
	}
}