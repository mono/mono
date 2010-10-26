// CS0508: `A<T>.B<H>.Test()': return type must be `A<T>.B<H>' to match overridden member `A<A<T>.B<H>>.Test()'
// Line: 10

abstract class A<T>
{
	public abstract T Test ();

	public class B<H> : A<B<H>>
	{
		public override B<H> Test ()
		{
			return this;
		}
	}
}
