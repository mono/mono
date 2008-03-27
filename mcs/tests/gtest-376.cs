using System;

struct S<T> where T : struct
{
	static object Box (T? o)
	{
		if (o == null)
			return null;
		return (T) o;
	}
}

class C
{
	public static void Main ()
	{
	}
}
