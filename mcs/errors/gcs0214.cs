// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 9
// Compiler options: -unsafe

public class aClass
{
	public struct foo_t
	{
		public fixed char b[16];
	}
}
