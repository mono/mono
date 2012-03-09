// CS4001: Cannot await `int' expression
// Line: 8

class A
{
	static async void Test ()
	{
		await 1;
	}
}
