// CS7082: The CallerFilePathAttribute applied to parameter `o' will have no effect. It is overridden by the CallerLineNumberAttribute
// Line: 9
// Compiler options: -warnaserror

using System.Runtime.CompilerServices;

class D
{
	void Foo ([CallerFilePath, CallerLineNumber] object o = null)
	{
	}
}