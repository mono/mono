// CS0007: Keyword `this' is not available in the current context
// Line: 14

class B
{
	public B (object o)
	{
	}
}

class C : B
{
	public C ()
		: base (this)
	{
	}
}

