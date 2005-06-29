// cs0619-15.cs: `Test.Test()' is obsolete: `Causes an error'
// Line: 9

using System;
public class Test
{
	[Obsolete ("Causes an error", true)]
	public Test () {}
	public Test (bool flag) : this () {}
}