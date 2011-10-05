// CS1992: The `await' operator can only be used when its containing method or lambda expression is marked with the `async' modifier
// Line: 10
// Compiler options: -langversion:future

using System.Threading.Tasks;

class Tester
{
	void Test ()
	{
		Task<int> x = null;
		var a = await x;
	}
}
