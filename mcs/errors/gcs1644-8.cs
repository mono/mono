// CS1644: Feature `query expressions' cannot be used because it is not part of the C# 2.0 language specification
// Line: 11
// Compiler options: -langversion:ISO-2

using System.Linq;

public class C
{
	public static void Main ()
	{
		var e = from a in "aaa" select a;
	}
}
