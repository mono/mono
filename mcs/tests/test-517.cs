// Compiler options: -warnaserror -warn:2

class Test {
	public static int Main ()
	{
		int i = 1;
		goto lbl;
	lbl:
		i = 0;
		return i;
	}
}
