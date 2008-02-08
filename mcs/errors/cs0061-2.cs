// CS0061: Inconsistent accessibility: base interface `B.IBase' is less accessible than interface `A.IDerived'
// Line: 6

public class A
{
	protected interface IDerived : B.IBase
	{
	}
}

public class B
{
	protected internal interface IBase 
	{
	}
}
