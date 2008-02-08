// CS0060: Inconsistent accessibility: base class `B.C.E' is less accessible than class `B.A.D'
// Line: 15

public class B
{
	protected class C : A
	{
		public class E
		{
		}
	}

	protected internal class A
	{
		protected class D : C.E
		{
		}
	}
}
