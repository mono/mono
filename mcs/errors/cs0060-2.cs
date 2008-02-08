// CS0060: Inconsistent accessibility: base class `A.B.C' is less accessible than class `A.B.D'
// Line: 10

public class A
{
	private class B
	{
		protected class C { }

		protected internal class D : C
		{
		}
    }
}
