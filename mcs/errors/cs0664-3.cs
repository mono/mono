// CS0664: Literal of type double cannot be implicitly converted to type `float'. Add suffix `f' to create a literal of this type
// Line: 13

using System;

public sealed class BoundAttribute : System.Attribute
{
	public float D;
}

class C
{
	[Bound (D = 300d)]
	double d2;
}