// CS1969: Dynamic operation cannot be compiled without `Microsoft.CSharp.dll' assembly reference
// Line: 19
// Compiler options: -noconfig

using System;

namespace System.Runtime.CompilerServices
{
	class DynamicAttribute : Attribute
	{
	}
}

class C
{
	public static void Main ()
	{
		dynamic d = null;
		d++;
	}
}
