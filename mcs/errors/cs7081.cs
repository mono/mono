// CS7081: The CallerMemberNameAttribute applied to parameter `o' will have no effect. It is overridden by the CallerLineNumberAttribute
// Line: 9
// Compiler options: -warnaserror

using System.Runtime.CompilerServices;

class D
{
	void Foo ([CallerMemberName, CallerLineNumber] object o = null)
	{
	}
}