using System;

public class GenericType<U> where U : IEquatable<U>
{
	public U u;
}

public class Base
{
	public virtual T Test<T> (GenericType<T> gt) where T : IEquatable<T>
	{
		return gt.u;
	}
}

public class Override : Base
{
	public override T Test<T> (GenericType<T> gt)
	{
		return base.Test (gt);
	}

	public static int Main ()
	{
		Base b = new Override ();
		b.Test (new GenericType<int> ());
		return 0;
	}
}
