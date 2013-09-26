// This ensures that any "unreachable code" warning will error out
// rather than generate invalid IL or crash compiler

using System;

class Foo
{
	public static int y = 1;
	public static int f () { return 0; }
	public static int Main ()
	{
		int x;

		do {
			x = f ();
			if (x != 0)
				continue;
			return 0;
		} while (x > y);

		return 1;
	}

	public static string Test_2 ()
	{
		throw new Exception ();

		var account = "yo";
		if (account == null) {
		}

		var s = "yo";

		switch (8) {
		case 1:
		case 2:
			break;
		default:
			throw new NotSupportedException ();
		}

		return s;
	}
}
