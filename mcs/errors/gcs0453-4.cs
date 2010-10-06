// CS0453: The type `T' must be a non-nullable value type in order to use it as type parameter `T' in the generic type or method `System.Nullable<T>'
// Line: 14

using System;

public static class Nullable_Test {
	public static int Compare<T> (Nullable<T> left)
	{
		return 0;
	}
}