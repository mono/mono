public class Test
{
	public static void InsertAt<T> (T[] array, int index, params T[] items)
	{
	}
	
	public static void Main()
	{
		int[] x = new int[] {1, 2};
		int[] y = new int[] {3, 4};
		InsertAt(x, 0, y);
	}
}

