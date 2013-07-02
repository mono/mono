// Compiler options: -warnaserror -w:2

using System;
class X {

	public static void HandleConflict (int a) {
		if (a != 1)
			goto throwException;
		if (a != 2)
			goto throwException;
		return;
	throwException:
		throw new Exception ();
	}

        public static int Main ()
	{
		int ret = 1;
		try { HandleConflict (1); }
		catch {
			try { HandleConflict (2); }
			catch { ret = 0; }
		}
		return ret;
	}
}
