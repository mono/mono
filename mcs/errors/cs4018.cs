// CS4018: The CallerFilePath attribute cannot be applied because there is no standard conversion from `string' to `int'
// Line: 8

using System.Runtime.CompilerServices;

class C
{
	public void Trace([CallerFilePath] int member = 0)
	{
	}
}
