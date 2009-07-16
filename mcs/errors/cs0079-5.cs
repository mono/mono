// CS0079: The event `C.ev' can only appear on the left hand side of `+=' or `-=' operator
// Line: 14

class C
{
	static event System.EventHandler ev
	{
		add { }
		remove { }
	}

	static void Main ()
	{
		ev *= null;
	}
}
