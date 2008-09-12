// CS0754: A partial method `C.I.Foo()' cannot explicitly implement an interface
// Line: 12


public interface I
{
	void Foo ();
}

public partial class C : I
{
	partial void I.Foo ();
}
