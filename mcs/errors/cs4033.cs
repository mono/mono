// CS4033: The `await' operator can only be used when its containing method is marked with the `async' modifier
// Line: 11

using System.Threading.Tasks;

class Tester
{
	void Test ()
	{
		Task<int> x = null;
		var a = await x;
	}
}
