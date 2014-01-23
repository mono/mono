using System;

public interface IFoo
{
	IFoo Hello ();
}

public interface IFoo<T> : IFoo
{
	new IFoo<T> Hello ();
}

public interface ICollectionValue<T>: IFoo<T>
{
}

public interface ICollection<T>: ICollectionValue<T>
{ }

public abstract class EnumerableBase<T> : IFoo<T>
{
	public abstract IFoo<T> Hello();

	IFoo IFoo.Hello ()
	{
		return Hello ();
	}
}

public abstract class CollectionBase<T> : EnumerableBase<T>
{
}

public class HashBag<T>: CollectionBase<T>, ICollection<T>
{
	public override IFoo<T> Hello ()
	{
		return this;
	}
}

class X
{
	public static void Main ()
	{
	}
}
