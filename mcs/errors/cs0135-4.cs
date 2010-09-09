// CS0135: `test' conflicts with a declaration in a child block
// Line: 13

class ClassMain
{
	static bool test = true;

	public static void Main ()
	{
		if (true) {
			const bool test = false;
		}
		test = false;
	}
}

