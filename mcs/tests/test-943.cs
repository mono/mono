using System;

public struct MyStruct
{
	public int X { get; set; }
}

class X
{
	public static int Main ()
	{
		var s = typeof (MyStruct);

		if (s.StructLayoutAttribute.Size != 0)
			return 1;

		return 0;
	}
}