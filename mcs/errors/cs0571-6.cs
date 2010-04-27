// CS0571: `IFoo.this[int].get': cannot explicitly call operator or accessor
// Line: 11
// Compiler options: -r:CS0571-6-lib.dll

using System;

public class Test
{
	void TestMethod (IFoo i)
	{
		i.get_Jaj (1);
	}
}
