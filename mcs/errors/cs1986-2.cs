// CS1986: The `await' operand type `int' must have suitable GetAwaiter method
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
	static async void Test ()
	{
		await 1;
	}
}