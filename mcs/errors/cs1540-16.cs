// CS1540: Cannot access protected member `Parent.Foo()' via a qualifier of type `T'. The qualifier must be of type `Child<T>' or derived from it
// Line: 9
// Compiler options: -r:CS1540-15-lib.dll

public class Child<T> : Parent where T : Parent
{
	public void Bar (T p)
	{
		p.Foo ();
	}
}
