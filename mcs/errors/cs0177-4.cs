// CS0177: The out parameter `f' must be assigned to before control leaves the current method
// Line: 5

class C {
	public static void test (int a, out float f)
	{
		do {
			// CS0177
			if (a == 8) {
				System.Console.WriteLine ("Hello");
				return;
			}
		} while (false);

		f = 1.3F;
		return;
	}
}

