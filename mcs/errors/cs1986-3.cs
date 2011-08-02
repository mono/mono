// CS1986: The `await' operand type `A' must have suitable GetAwaiter method
// Line: 15
// Compiler options: -langversion:future

static class S
{
	public static void GetAwaiter (this int i)
	{
	}
}

class A
{
	bool GetAwaiter;
	
	static async void Test ()
	{
		await new A ();
	}
}