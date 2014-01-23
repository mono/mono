// Compiler options: -warnaserror

class Foo {
	public static int Main ()
	{
		for (;;) {
			try {
				break;
			} catch {
				continue;
			}
		}
		return 0;
	}
}
