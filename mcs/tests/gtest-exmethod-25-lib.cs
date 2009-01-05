// Compiler options: -target:library

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo ("gtest-exmethod-25")]

namespace lib1
{
	static class Foo
	{
		internal static void Extend (this string aString)
		{
		}
	}
}
