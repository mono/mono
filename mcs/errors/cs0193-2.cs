// CS0193: The * or -> operator must be applied to a pointer
// Line: 10
// Compiler options: -unsafe

class C
{
	unsafe static void Foo (object o)
	{
		bool x = false;
		if (o is bool ? *x : null) {
		}
	}
}
