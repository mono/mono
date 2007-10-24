// CS0764: A partial method declaration and partial method implementation must be both `unsafe' or neither
// Line: 11
// Compiler options: -langversion:linq -unsafe

public partial class C
{
	unsafe partial void Foo ()
	{
	}
	
	partial void Foo ();
}
