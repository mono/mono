// CS0108: `IB.Foo(int)' hides inherited member `IA.Foo'. Use the new keyword if hiding was intended
// Line: 13
// Compiler options: -warnaserror

interface IA
{
	bool Foo { get; }
}

interface IB : IA
{
	new void Foo ();
	void Foo (int a);
}
