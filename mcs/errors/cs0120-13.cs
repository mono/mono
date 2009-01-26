// CS0120: An object reference is required to access non-static member `C.Test(string)'
// Line: 8

class C
{
	static void Test (int i)
	{
		Test ("a");
	}
	
	void Test (string s)
	{
	}
}
