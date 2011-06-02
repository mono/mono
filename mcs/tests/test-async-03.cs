// Compiler options: -langversion:future

using System.Threading.Tasks;

static class S
{
	public static A GetAwaiter (this int i)
	{
		return new A ();
	}
}

class A
{
	bool IsCompleted {
		get {
			return true;
		}
	}
	
	void OnCompleted (System.Action a)
	{
	}
	
	int GetResult ()
	{
		return 3;
	}
	
	static async Task<int> Test1 ()
	{
		await checked (1);
		return await checked (2);
	}

	static async Task<int> Test2 ()
	{
		await checked (1);
		return 4;
	}
	
	static async Task Test3 ()
	{
		await checked (1);
	}

	public static int Main ()
	{
		var r = Test1 ();
		System.Console.WriteLine (r.Result);
		if (r.Result != 3)
			return 1;

		r = Test2 ();
		System.Console.WriteLine (r.Result);
		if (r.Result != 4)
			return 2;
		
		Test3();
		return 0;
	}
}