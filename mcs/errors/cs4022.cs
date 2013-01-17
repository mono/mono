// CS4022: The CallerMemberName attribute can only be applied to parameters with default value
// Line: 8

using System.Runtime.CompilerServices;

class C
{
	public void Trace([CallerMemberName] string member)
	{
	}
}
