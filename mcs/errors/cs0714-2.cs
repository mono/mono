// cs0714.cs: `StaticClass': static classes cannot implement interfaces
// Line: 8

static partial class StaticClass
{
}

static partial class StaticClass: System.ICloneable
{
}
