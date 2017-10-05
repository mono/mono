// CS8148: `B.Foo()': must not return by reference to match overridden member `A.Foo()'
// Line: 11

public abstract class A
{
	public abstract int Foo ();
}

public class B : A
{
	public override ref int Foo ()
	{

	}
}