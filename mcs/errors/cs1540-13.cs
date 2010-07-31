// CS1540: Cannot access protected member `A.del' via a qualifier of type `A'. The qualifier must be of type `B' or derived from it
// Line: 16

delegate int D ();

class A
{
	protected D del;
}

class B : A
{
    public static void Main ()
	{
		A b = new A ();
		var v = b.del ();
	}
}
