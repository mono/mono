// CS0220: The operation overflows at compile time in checked mode
// Line: 10
// Compiler options: -unsafe

class TestClass
{
	public unsafe static void Main ()
	{
		int* arr = null;
		var i = arr[long.MaxValue];
	}
}