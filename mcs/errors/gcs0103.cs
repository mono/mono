// CS0103: The name `Console' does not exist in the current context
// Line: 10
// Compiler options: -langversion:linq

class C
{
	delegate void WithOutParam (string value);

	static void Main() 
	{
		WithOutParam o = (s) => Console.WriteLine();
	}
}
