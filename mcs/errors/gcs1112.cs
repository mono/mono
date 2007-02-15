// CS1112: Do not use `System.Runtime.CompilerServices.ExtensionAttribute' directly. Use parameter modifier `this' instead
// Line: 8
// Compiler options: -langversion:linq

using System.Runtime.CompilerServices;

static class C
{
	[Extension]
	static void Foo (this string s)
	{
	}
}