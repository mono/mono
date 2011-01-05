// CS0051: Inconsistent accessibility: parameter type `C.S?[][]' is less accessible than method `C.Foo(C.S?[][])'
// Line: 7

public class C
{
	struct S {}
	public void Foo (S?[][] o) {}
}
