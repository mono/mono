// CS0162: Unreachable code detected
// Line: 10
// Compiler options: -warnaserror

class C
{
	void Test (int a)
	{
		return;
		if (a > 0) {
			int x = a + 20;
			return;
		}
	}
}