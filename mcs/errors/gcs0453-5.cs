// CS0453: The type `B.M' must be a non-nullable value type in order to use it as type parameter `T' in the generic type or method `Foo.Test_2<T>(this T)'
// Line: 20


using System;

public static class Foo
{
	public static string Test_2<T> (this T s) where T : struct
	{
		return null;
	}
}

namespace B
{
	public class M
	{
		public static void Main ()
		{
			new M().Test_2();
		}
	}
}