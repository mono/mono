// CS4019: The CallerMemberName attribute cannot be applied because there is no standard conversion from `string' to `int'
// Line: 8

using System.Runtime.CompilerServices;

class C
{
	public void Trace([CallerMemberName] int member = 0)
	{
	}
}
