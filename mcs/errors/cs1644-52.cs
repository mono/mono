// CS1644: Feature `pattern matching' cannot be used because it is not part of the C# 6.0 language specification
// Line: 9
// Compiler options: -langversion:6

class Class
{
	static void Foo (object arg)
	{
		if (arg is Type v) {
			return;
		}
	}	
}
