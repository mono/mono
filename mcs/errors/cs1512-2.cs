// CS1512: Keyword `base' is not available in the current context
// Line: 11

struct S
{
	delegate int D ();
	
	void Test ()
	{
		D d = delegate { 
			return base.GetHashcode ();
		};
	}
}
