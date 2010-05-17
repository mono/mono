// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 11
// Compiler options: -unsafe

public class C
{
	unsafe int* i;
	
	public static void Main ()
	{
		var v = new C().i;
	}
}
