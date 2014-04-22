// CS9005: Constructor initializer cannot access primary constructor parameters
// Line: 7

class Test(string s)
{
	public Test ()
		: this (s)
	{
	}
}