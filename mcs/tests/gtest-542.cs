abstract class A<T>
{
	public abstract T getT ();

	public class B : A<B>
	{
		public override A<T>.B getT ()
		{
			return this;
		}
	}
}

class C
{
	static int Main ()
	{
		var r = new A<short>.B ();
		if (r.getT () != r)
			return 1;
		
		return 0;
	}
}
