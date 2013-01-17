using System;

public class C
{
	public static int Main ()
	{
		try {
			throw new ArgumentException ();
		} catch (ArgumentException) {
			return 0;
		} catch {
			return 1;
		}
	}
}