using System;

public class GenericType<U, V> where U : IEquatable<U> where V : IEquatable<V>
{
	public U u;
}

public class Base<V> where V : IEquatable<V>
{
	public virtual T Test<T> (GenericType<T, V> gt) where T : IEquatable<T>
	{
		return gt.u;
	}
}

public class Override<W> : Base<W> where W : IEquatable<W>
{
	public override T Test<T> (GenericType<T, W> gt)
	{
		return base.Test (gt);
	}
}

class M
{
	public static int Main ()
	{
		Base<byte> b = new Override<byte> ();
		b.Test (new GenericType<int, byte> ());
		return 0;
	}
}
