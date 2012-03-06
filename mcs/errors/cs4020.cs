// CS4020: The CallerLineNumber attribute can only be applied to parameters with default value
// Line: 8

using System.Runtime.CompilerServices;

class C
{
	public void Trace([CallerLineNumber] int member)
	{
	}
}
