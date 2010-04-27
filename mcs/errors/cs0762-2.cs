// CS0762: Cannot create delegate from partial method declaration `C.Foo()'
// Line: 12

partial class C
{
	delegate void D ();

	partial void Foo ();

	void Test ()
	{
		D d = Foo;
	}
}
