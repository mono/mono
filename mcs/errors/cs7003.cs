// CS7003: Unbound generic name is not valid in this context
// Line: 17

using System;

class C<T>
{
	public class G<U>
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