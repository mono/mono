using System;

public class Foo
{
	public static int Main ()
	{
		try {
			lock (null) {
				return 1;
			}
		} catch (ArgumentNullException) {
		}

		for (int i = 0; i < 3; ++i) {
			object token = new object ();

			lock (token)
			{
				token = null;
			}
		}

		return 0;
	}
}