// CS0177: The out parameter `f' must be assigned to before control leaves the current method
// Line: 5

class C {
	public static void test3 (out float f)
	{
		try {
			f = 8.53F;
		} catch {
			return;
		}
	}
}

