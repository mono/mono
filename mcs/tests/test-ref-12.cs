// Compiler options: -unsafe

unsafe class X
{
	public static void Main ()
	{
		void* pointer = null;
 		Bar (ref Foo (ref *(byte*)pointer));
	}

	static int field;

	static ref int Foo (ref byte b)
	{
		return ref field;
	}

	static void Bar (ref int i)
	{

	}
}