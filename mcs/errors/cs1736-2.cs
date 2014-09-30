// CS1736: The expression being assigned to optional parameter `s' must be a constant or default value
// Line: 11

struct S
{
	public S ()
	{		
	}
}

class X
{
	public void Foo (S s = new S ())
	{
	}
}