// CS0060: Inconsistent accessibility: base class `X.A.D.E' is less accessible than class `X.A.B.F'
// Line: 10

internal class X
{
	protected class A
	{
		protected internal class B
		{
			internal class F : D.E
			{
			}
		}

		protected class D : B
		{
			internal class E { }
		}
	}
}
