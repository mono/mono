// CS1112: Do not use `System.Runtime.CompilerServices.ExtensionAttribute' directly. Use parameter modifier `this' instead
// Line: 16


using System;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
	public class ExtensionAttribute : Attribute
	{
	}
}

static class C
{
	[Extension]
	static void Foo (this string s)
	{
	}
}