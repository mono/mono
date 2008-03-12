// CS0656: The compiler required member `System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray(System.Array, System.RuntimeFieldHandle)' could not be found or is inaccessible
// Line: 16
// Compiler options: -nostdlib CS0656-corlib.cs

namespace System.Runtime.CompilerServices {
	class RuntimeHelpers
	{
		public static void InitializeArray ()
		{
		}
	}
}

class C
{
	int[] ff = new int[] { 1, 3, 4, 5, 6, 7, 8, 10, 22, 22, 233, 44, 55, 66 };
}
