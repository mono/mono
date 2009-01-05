// Compiler options: -target:library

using System.Runtime.CompilerServices;

namespace lib1
{
	internal static class Foo
	{
		// It compiles fine, if I make this a non-extension-method:
		public static void Extend (this string aString)
		{
		}
	}
}
