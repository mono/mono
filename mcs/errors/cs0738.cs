// CS0738: `B' does not implement interface member `I.Test(int)' and the best implementing candidate `B.Test(int)' return type `int' does not match interface member return type `void'
// Line: 9

interface I
{
	void Test (int a);
}

class B: I
{
	public int Test (int a)
	{
		return a;
	}
}
