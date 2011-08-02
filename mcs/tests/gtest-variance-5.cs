interface ICovariant<out T>
{
	T Foo { get; }
}

interface IContravariant<in T>
{
	int Bar (T bar);
}

interface IBothVariants <out T1, in T2> : ICovariant<T1>, IContravariant<T2>
{
}

interface IInvariant <T> : ICovariant<T>, IContravariant<T>
{
}

class BothVariants <T1, T2> : IBothVariants <T1, T2>
{
	public BothVariants (T1 foo)
	{
		Foo = foo;
	}

	public T1 Foo { get; private set; }

	public int Bar (T2 bar)
	{
		return bar.GetHashCode () ^ Foo.GetHashCode ();
	}
}

class Invariant <T> : IInvariant<T> where T : new()
{
	public T Foo { get { return new T (); } }

	public int Bar (T bar)
	{
		return bar.GetHashCode ();
	}
}

class A
{
	public virtual string Fruit { get { return "Apple"; } }
}

class B : A
{
	public override string Fruit { get { return "Banana"; } }
}

class C : B
{
	public override string Fruit { get { return "Carrot which I know is not a fruit but you better shut up about it before I cut you"; } }
}

public class Test
{
	static int Main ()
	{
		var b = new B ();
		var c = new C ();

		IBothVariants<A, C> both = new BothVariants<B,B> (b);

		if (both.Bar (c) != (b.GetHashCode () ^ c.GetHashCode ()))
			return 1;

		IInvariant<B> neither = new Invariant<B> ();
		ICovariant<A> co = neither;
		if (co.Foo.Fruit != "Banana")
			return 2;

		IContravariant<C> contra = neither;
		if (contra.Bar (c) != c.GetHashCode ())
			return 3;

		return 0;
	}
}
