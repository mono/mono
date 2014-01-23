using System.Linq;
using System;

class C
{
	static object Method <T>(object[] objects)
	{
		return objects.Select(obj => new Func<T, object>(x => obj));
	}

	public static void Main ()
	{
		Method<string> (new[] { "a", "b", "c" });
	}
}