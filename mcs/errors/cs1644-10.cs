// CS1644: Feature `namespace alias qualifier' cannot be used because it is not part of the C# 1.0 language specification
// Line: 7
// Compiler options: -langversion:ISO-1

class Program
{
	static void Main ()
	{
		System.Type t = typeof (global::System.Int32);
	}
}
