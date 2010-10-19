// cs0037.cs: Cannot convert null to `bool' because it is a value type
// Line: 13

using System;

public sealed class BoundAttribute : System.Attribute
{
	public bool Dec { set { } get { return false; } }
}

class C
{
	[Bound (Dec = null)]
	double d2;
}