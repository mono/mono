// CS4003: `await' cannot be used as an identifier within an async method or lambda expression
// Line: 8
// Compiler options: -langversion:future

class Tester
{
	async void Test ()
	{
		int await = 1;
	}
}
