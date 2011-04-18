// CS0592: The attribute `TestAttribute' is not valid on this declaration type. It is valid on `constructor' declarations only
// Line: 6

using System;

[TestAttribute ()]
enum E {
}

[AttributeUsage(AttributeTargets.Constructor)]
public class TestAttribute: Attribute {
}