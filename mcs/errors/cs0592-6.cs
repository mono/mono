// cs0592-6.cs: The attribute `TestAttribute' is not valid on this declaration type. It is valid on `constructor' declarations only
// Line: 6

using System;

[TestAttribute ()]
delegate void D ();

[AttributeUsage(AttributeTargets.Constructor)]
public class TestAttribute: Attribute {
}