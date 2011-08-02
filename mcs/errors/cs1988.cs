// CS1988: Async methods cannot have ref or out parameters
// Line: 6
// Compiler options: -langversion:future

class C
{
	public async void Test (ref int arg)
	{
	}
}
