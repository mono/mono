// CS0762: Cannot create delegate from partial method declaration `C.Foo()'
// Line: 12

partial class C
{
	delegate void D ();

	partial void Foo ();

	static void Main ()
	{
		D d = new D (new C ().Foo);
	}
}
