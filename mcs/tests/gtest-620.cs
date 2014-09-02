interface I<T>
{
}

class A<T>
{
	public virtual T M<U> (U u) where U : T
	{
		return u;
	}
}

class B<W> : A<I<W>>, I<string>
{
	public override I<W> M<U> (U u)
	{
		return u;
	}
}

class Bug
{
	public static void Main ()
	{
		var b = new B<string> ();
		b.M (b);
	}
}