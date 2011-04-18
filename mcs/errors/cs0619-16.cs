// CS0619: `Test_A.Test_A()' is obsolete: `Causes an error'
// Line: 13

using System;
public class Test_A
{
	[Obsolete ("Causes an error", true)]
	public Test_A () {}
}

public class Test_B: Test_A
{
	public Test_B (): base () {}
}