
public class Z : IGenericInterface<Z>
{
	public void Stop ()
	{
	}

	Z IGenericInterface<Z>.Start ()
	{
		return this;
	}
}

public interface IGenericInterface<T>
{
	T Start ();
}

public class A<Y, Y2, W>
	where Y : Z, IGenericInterface<Y>
	where Y2 : class
	where W : Y, Y2
{
	public void SomeOperation (W w)
	{
		w.Start ();
		w.Stop ();
	}

	public void SomeOtherOperation (Y y)
	{
		y.Start ();
		y.Stop ();
	}
}

public class Foo
{
	public static int Main ()
	{
		var a = new A<Z, object, Z> ();
		a.SomeOperation (new Z ());
		a.SomeOtherOperation (new Z ());
		return 0;
	}
}
