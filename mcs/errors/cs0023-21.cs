// CS0023: The `?' operator cannot be applied to operand of type `int*'
// Line: 10
// Compiler options: -unsafe

class C
{
	unsafe static void Main ()
	{
		int* arr = null;
		var v2 = arr?.ToString ();
	}
}