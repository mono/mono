// CS0452: The type `int' must be a reference type in order to use it as type parameter `T' in the generic type or method `Program.M<T>(T, T)'
// Line: 8

class Program
{
	public static void M<T> (T item1, T item2 = null) where T : class
	{
		M (1);
	}
}