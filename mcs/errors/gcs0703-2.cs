// CS0703: Inconsistent accessibility: constraint type `A.B<T>.C' is less accessible than `A.B<T>'
// Line: 6

public class A
{
	protected internal class B<T> where T : B<T>.C
	{
		internal class C
		{
		}
	}
}
