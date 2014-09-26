// CS1736: The expression being assigned to optional parameter `s' must be a constant or default value
// Line: 11

struct S
{
	public int i = 8;
}

class X
{
	public void Foo (S s = new S ())
	{
	}
}