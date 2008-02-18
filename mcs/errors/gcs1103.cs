// CS1103: The type of extension method cannot be `int*'
// Line: 7
// Compiler options: -unsafe

static class S
{
	unsafe static void Foo (this int* iptr)
	{
	}
}
