// CS0052: Inconsistent accessibility: field type `A.B' is less accessible than field `A.C.D.b'
// Line: 12

public class A
{
	private class B { }

	public class C
	{
		protected class D
		{
			protected B b;
		}
	}
}
