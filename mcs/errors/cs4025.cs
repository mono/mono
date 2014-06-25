// CS4025: The CallerFilePath applied to parameter `x' will have no effect because it applies to a member that is used in context that do not allow optional arguments
// Line: 14
// Compiler options: -warnaserror

using System.Runtime.CompilerServices;

partial class D
{
	partial void Foo (string x = "x");
}

partial class D
{
	partial void Foo ([CallerFilePath] string x)
	{
	}
}