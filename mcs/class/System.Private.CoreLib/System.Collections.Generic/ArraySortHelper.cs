namespace System.Collections.Generic
{
	partial class ArraySortHelper<T>
	{
		public static ArraySortHelper<T> Default { get; } = new ArraySortHelper<T>();
	}

	partial class ArraySortHelper<TKey, TValue>
	{
		public static ArraySortHelper<TKey, TValue> Default { get; } = new ArraySortHelper<TKey, TValue>();
	}
}
