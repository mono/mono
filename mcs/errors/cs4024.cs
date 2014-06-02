// CS4024: The CallerLineNumberAttribute applied to parameter `x' will have no effect because it applies to a member that is used in context that do not allow optional arguments
// Line: 14
// Compiler options: -warnaserror

using System.Runtime.CompilerServices;

partial class D
{
	partial void Foo (int x = 2);
}

partial class D
{
	partial void Foo ([CallerLineNumber] int x)
	{
	}
}