// cs0536-2.cs: `B' does not implement interface member `I.Test(int)'. `B.Test(int)' is either static, not public, or has the wrong return type
// Line: 11

interface I
{
	void Test (int a);
}

class B: I
{
	public static void Test (int a) {}
}
