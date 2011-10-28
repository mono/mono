// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `S<int>.N'
// Line: 16
// Compiler options: -unsafe

struct S<T>
{
	public struct N
	{
	}
}

unsafe class Test
{
	public static void Main()
	{
		S<int>.N* a;
	}
}
