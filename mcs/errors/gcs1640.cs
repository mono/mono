// CS1640: foreach statement cannot operate on variables of type `T' because it contains multiple implementation of `System.Collections.Generic.IEnumerable<T>'. Try casting to a specific implementation
// Line: 11

using System.Collections.Generic;

public class C
{
	public static void Test<T>(T t) 
		where T: IEnumerable<string>, IEnumerable<int>
	{
		foreach (int i in t)
		{
		}
	}
}
