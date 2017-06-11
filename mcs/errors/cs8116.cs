// CS8116: The nullable type `byte?' pattern matching is not allowed. Consider using underlying type `byte'
// Line: 11

using System;

class C
{
	public static void Main ()
	{
		object o2 = null;
		bool r2 = o2 is Nullable<byte> t3;
	}
}