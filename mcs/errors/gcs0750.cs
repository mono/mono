// CS0750: A partial method cannot define access modifier or any of abstract, extern, new, override, sealed, or virtual modifiers
// Line: 7
// Compiler options: -langversion:linq

public partial class C
{
	private partial void Foo ()
	{
	}
}
