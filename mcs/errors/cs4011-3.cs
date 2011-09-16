// CS4011: The awaiter type `A' must have suitable IsCompleted, OnCompleted, and GetResult members
// Line: 16
// Compiler options: -langversion:future

static class S
{
	public static A GetAwaiter (this int i)
	{
		return new A ();
	}
}

class A
{
	int IsCompleted {
		get {
			return 1;
		}
	}
	
	static async void Test ()
	{
		await 9;
	}
}
