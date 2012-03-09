// CS4021: The CallerFilePath attribute can only be applied to parameters with default value
// Line: 8

using System.Runtime.CompilerServices;

class C
{
	public void Trace([CallerFilePath] string member)
	{
	}
}
