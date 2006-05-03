// Compiler options: -warnaserror

// This ensures that any "unreachable code" warning will error out
// rather than generate invalid IL

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
}
