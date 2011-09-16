// CS4011: The awaiter type `int' must have suitable IsCompleted, OnCompleted, and GetResult members
// Line: 16
// Compiler options: -langversion:future

static class S
{
	public static int GetAwaiter (this int i)
	{
		return 1;
	}
}

class A
{
	static async void Test ()
	{
		await 9;
	}
}