using System;

public class Tests
{
	enum MyEnumUlong : long
	{
		Value_1,
		Value_3 = 2
	}

	public static int Main ()
	{
		var d = MyEnumUlong.Value_1;
		d = d + (byte) 1;
		d += (byte) 1;

		if (d != MyEnumUlong.Value_3)
			return 1;

		return 0;
	}
}
