// CS1644: Feature `dictionary initializer' cannot be used because it is not part of the C# 5.0 language specification
// Line: 12
// Compiler options: -langversion:5

using System.Collections.Generic;

class C
{
	public static void Main ()
	{
		var d = new Dictionary<string, int> {
			["a"] = 1
		};
	}
}