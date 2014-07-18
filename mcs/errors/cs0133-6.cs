// CS0133: The expression being assigned to `o' must be constant
// Line: 8

class X
{
	void Foo ()
	{
		const object o = "" ?? null;
	}
}