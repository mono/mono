// CS0736: `B' does not implement interface member `I.Test(int)' and the best implementing candidate `B.Test(int)' is static
// Line: 11

interface I
{
	void Test (int a);
}

class B: I
{
	public static void Test (int a) {}
}
