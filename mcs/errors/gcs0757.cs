// CS0757: A partial method `C.Foo()' implementation is already defined
// Line: 11
// Compiler options: -langversion:linq

public partial class C
{
	partial void Foo ()
	{
	}
	
	partial void Foo ()
	{
	}
}
