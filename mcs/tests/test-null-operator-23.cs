using System;

public class X
{
	string field;

	public static int Main ()
	{
		X x = null;

		try {
			var res = (x?.field).ToString()?.Length;
			return 1;
		} catch (NullReferenceException) {

		}

		return 0;
	}
}