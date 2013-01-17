// CS4017: The CallerMemberName attribute cannot be applied because there is no standard conversion from `int' to `byte'
// Line: 8

using System.Runtime.CompilerServices;

class C
{
	public void Trace([CallerLineNumber] byte member = 1)
	{
	}
}
