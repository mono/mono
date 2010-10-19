// cs0029-2.cs: Cannot implicitly convert type `string' to `double'
// Line: 11

using System;

public sealed class BoundAttribute : System.Attribute
{
	public double D;
}

class C
{
	[Bound (D = "Dude!")]
	double d2;
}