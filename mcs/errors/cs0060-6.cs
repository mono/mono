// CS0060: Inconsistent accessibility: base class `A<C>.B' is less accessible than class `D'
// Line: 13

public class A<T>
{
	public class B {}
}

internal class C : A<C>
{
}

public class D : C.B
{
}
