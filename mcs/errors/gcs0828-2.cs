// CS0828: An anonymous type property `Value' cannot be initialized with `int*'
// Line: 9
// Compiler options: -unsafe

public unsafe class Test
{
	static int* Error ()
	{
		return (int*)0;
	}
	
	static void Main ()
	{
		var v = new { Value = Error () };
	}
}
