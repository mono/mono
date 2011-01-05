// CS0508: `A<T>.B.getT()': return type must be `A<T>.B' to match overridden member `A<A<T>.B>.getT()'
// Line: 10

abstract class A<T>
{
	public abstract T getT ();

	public class B : A<B>
	{
		public override B getT ()
		{
			throw new System.Exception ("The method or operation is not implemented.");
		}
	}
}
