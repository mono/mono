// CS4003: `await' cannot be used as an identifier within an async method or lambda expression
// Line: 8

class Tester
{
	int await;
	
	async void Test ()
	{
		var a = new Initializer () {
			await = 2
		};
	}
}
