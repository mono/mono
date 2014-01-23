class Test
{
	public static void Main () { }

	static void Foo<T> (ref T t) { }
	static void Foo<T> (T[] t) { }
	static void Foo<T> (T[,] t) { }
	static void Foo<T> (T[, ,] t) { }

	static void Bar (ref int t) { }
	static void Bar (int[] t) { }
	static void Bar (int[,] t) { }
	static void Bar (int[, ,] t) { }
}
