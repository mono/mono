// CS1745: Cannot specify `System.Runtime.InteropServices.DefaultParameterValue' attribute on optional parameter `u'
// Line: 8
// Compiler options: -langversion:future

using System.Runtime.InteropServices;

public class C
{
	public static void Test ([DefaultParameterValue (1)] int u = 2)
	{
	}
}
