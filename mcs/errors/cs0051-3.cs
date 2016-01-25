// CS0051: Inconsistent accessibility: parameter type `NV' is less accessible than method `C1.Foo(NV)'
// Line: 14

internal class NV
{
}

public partial class C1
{
}

partial class C1
{
	public void Foo (NV arg)
	{
	}
}