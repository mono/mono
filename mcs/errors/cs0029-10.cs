// CS0029: Cannot implicitly convert type `void' to `System.EventHandler'
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
		ev += ev += null;
	}
}
