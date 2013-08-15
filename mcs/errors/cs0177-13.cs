// CS0177: The out parameter `baz' must be assigned to before control leaves the current method
// Line: 6

static class A
{
	public static void Foo (int i, out object baz)
	{
		switch (i) {
		case 0:
			baz = 1;
			return;
		}
	}
}