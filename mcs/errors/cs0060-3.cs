// CS0060: Inconsistent accessibility: base class `A.B.Base' is less accessible than class `A.B.Derived'
// Line: 9

internal class A
{
	protected class B
	{
		protected class Base {}
		public class Derived : Base { }
	}
}
