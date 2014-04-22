// CS0029: Cannot implicitly convert type `string' to `int'
// Line: 10
// Compiler options: -unsafe

class TestClass
{
	public unsafe static void Main ()
	{
		int* arr = null;
		var i = arr["c"];
	}
}