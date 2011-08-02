// CS1986: The `await' operand type `int' must have suitable GetAwaiter method
// Line: 16
// Compiler options: -langversion:future

static class S
{
	public static int GetAwaiter<T> (this int i)
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