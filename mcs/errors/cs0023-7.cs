// CS0023: The `.' operator cannot be applied to operand of type `int*'
// Line: 8
// Compiler options: -unsafe

class C
{
	static unsafe int* Foo ()
	{
		return (int*)0;
	}
	
	public static void Main ()
	{
		unsafe {
			string s = Foo().ToString ();
		}
	}
}