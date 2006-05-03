// Compiler options: -warnaserror

class Foo {
	static int Main ()
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
