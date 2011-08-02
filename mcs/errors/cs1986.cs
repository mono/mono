// CS1986: The `await' operand type `int' must have suitable GetAwaiter method
// Line: 8
// Compiler options: -langversion:future

class A
{
	static async void Test ()
	{
		await 1;
	}
}