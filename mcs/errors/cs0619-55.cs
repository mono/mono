// CS0619: `ObsoleteInterface<T>' is obsolete: `'
// Line: 15

using System;

[Obsolete("", true)]
interface ObsoleteInterface<T>
{
}

class C
{
	public static void Main ()
	{
		var v = typeof (ObsoleteInterface<>);
	}
}
