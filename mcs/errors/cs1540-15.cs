// CS1540: Cannot access protected member `Parent.Foo()' via a qualifier of type `Parent'. The qualifier must be of type `Child' or derived from it
// Line: 8
// Compiler options: -r:CS1540-15-lib.dll

public class Child : Parent
{
	public void Bar (Parent p)
	{
		p.Foo ();
	}
}
