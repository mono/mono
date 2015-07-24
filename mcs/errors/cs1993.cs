// CS1993: Cannot find compiler required types for asynchronous functions support. Are you targeting the wrong framework version?
// Line: 38
// Compiler options: -nostdlib CS1993-corlib.cs

using System.Threading.Tasks;

namespace System.Threading.Tasks
{
	class Task<T>
	{
	}
}

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
	
	static async Task<int> Test ()
	{
		return await 2;
	}
}
