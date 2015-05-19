using System;

class X
{
	public static int Main ()
	{
		try {
			bool x = true;
			try {
				throw new ApplicationException ();
			} catch (NullReferenceException) when (x) {
				throw;
			}

			return 1;
		} catch (ApplicationException) {
			Console.WriteLine ("ok");
			return 0;
		}
	}
}