// CS4027: The awaiter type `A' must implement interface `System.Runtime.CompilerServices.INotifyCompletion'
// Line: 33

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
	
	public void OnCompleted (System.Action a)
	{
	}

	int GetResult ()
	{
		return 3;
	}
	
	static async Task<int> Test1 ()
	{
		await 1;
	}
}
