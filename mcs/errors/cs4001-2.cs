// CS4001: Cannot await `int' expression
// Line: 8
// Compiler options: -langversion:future

class A
{
	static async void Test ()
	{
		await 1;
	}
}
