// CS1031: Type expected
// Line: 17

using System;

class C<T>
{
	class G<U>
	{
	}
}

class M
{
	public static void Main ()
	{
		Type t = typeof (C<int>.G<>);
	}
}

