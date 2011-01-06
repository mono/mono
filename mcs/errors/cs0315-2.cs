// CS0315: The type `ushort' cannot be used as type parameter `T' in the generic type or method `A<T>'. There is no boxing conversion from `ushort' to `A<ushort>.N1<ushort>'
// Line: 9
// Compiler options: -r:CS0315-2-lib.dll

public class Test
{
	public static void Main ()
	{
		A<ushort>.N1<ushort> a = null;
	}
}
