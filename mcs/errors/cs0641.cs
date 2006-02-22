// cs0641.cs: Attribute `System.AttributeUsageAttribute' is only valid on classes derived from System.Attribute
// Line: 6

using System;

[AttributeUsage (AttributeTargets.All)]
public class A
{
}

