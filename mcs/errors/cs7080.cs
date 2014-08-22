// CS7080: The CallerMemberNameAttribute applied to parameter `o' will have no effect. It is overridden by the CallerFilePathAttribute
// Line: 9
// Compiler options: -warnaserror

using System.Runtime.CompilerServices;

class D
{
	void Foo ([CallerMemberName, CallerFilePath] object o = null)
	{
	}
}