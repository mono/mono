// CS8142: A partial method declaration and partial method implementation must both use the same tuple element names
// Line: 11

partial class X
{
	partial void Foo ((int a, int b) arg);
}

partial class X
{
	partial void Foo ((int c, int d) arg)
	{
	}
}