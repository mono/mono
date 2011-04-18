// CS0177: The out parameter `f' must be assigned to before control leaves the current method
// Line: 5

class ClassMain {
	public static void test2 (int a, out float f)
	{
		// CS0177
		if (a == 5)
			return;

		f = 8.53F;
	}
}

