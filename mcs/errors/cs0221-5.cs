// cs0221.cs: Constant value `-1' cannot be converted to a `byte' (use `unchecked' syntax to override)
// Line: 11

using System;

public class My3Attribute : Attribute
{
	public My3Attribute (byte b) {}
}

[My3((byte)-1)]
public class Test { }