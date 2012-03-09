// CS1986: The `await' operand type `A' must have suitable GetAwaiter method
// Line: 17

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