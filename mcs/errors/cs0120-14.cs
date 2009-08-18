// CS0120: An object reference is required to access non-static member `C.i'
// Line: 16

class B
{
	public B (object o)
	{
	}
}

class C : B
{
	int i;
	
	public C ()
		: base (i)
	{
	}
}

