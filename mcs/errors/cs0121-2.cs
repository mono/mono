// CS0121: The call is ambiguous between the following methods or properties: `IFoo.DoIt()' and `IBar.DoIt()'
// Line: 9

class A : IFooBar {
	static void Main ()
	{
		A a = new A ();
		IFooBar fb = (IFooBar) a;
		fb.DoIt ();
	}

	void IFoo.DoIt ()
	{
		System.Console.WriteLine ("void IFoo.DoIt ()");
	}

	void IBar.DoIt ()
	{
		System.Console.WriteLine ("void IBar.DoIt ()");
	}
}

interface IFoo {
	void DoIt ();
}

interface IBar {
	void DoIt ();
}

interface IFooBar : IFoo, IBar {}