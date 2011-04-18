// CS0592: The attribute `TestAttribute' is not valid on this declaration type. It is valid on `constructor' declarations only
// Line: 5

using System;
[assembly:TestAttribute ()]

[AttributeUsage(AttributeTargets.Constructor)]
public class TestAttribute: Attribute {
}