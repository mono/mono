// cs0641.cs: Attribute 'AttributeUsage' is only valid on classes derived from System.Attribute
// Line: 6

using System;

[AttributeUsage (AttributeTargets.All)]
public class A
{
}

