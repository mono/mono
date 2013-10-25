using System;

public class A
{
	public virtual T Test<T> (T t)
	{
		throw new ApplicationException ();
	}
}

public class B : A
{
	public override T Test<T> (T t)
	{
		Console.WriteLine ("Base");
		return default (T);
	}
}

public class C : B
{
	public override T Test<T> (T t)
	{
		base.Test ("a");
		return default (T);
	}
}


public class AG<U>
{
	public virtual T Test<T> (T t, U u)
	{
		throw new ApplicationException ();
	}
}

public class B<UB> : AG<UB>
{
	public override T Test<T> (T t, UB u)
	{
		Console.WriteLine ("Base");
		return default (T);
	}
}

public class C<UC> : B<UC>
{
	public override T Test<T> (T t, UC u)
	{
		base.Test ("a", default (UC));
		return default (T);
	}
}

class X
{
	public static void Main ()
	{
		new C ().Test<int> (1);
		new C<int> ().Test (5, 3);
	}
}