using System.Collections.Generic;
using System;

public class Factory<TKey, TBase>
{
	delegate T InstantiateMethod<T> ();

	Dictionary<TKey, InstantiateMethod<TBase>> _Products = new Dictionary<TKey, InstantiateMethod<TBase>> ();

	public void Register<T> (TKey key) where T : TBase, new()
	{
		_Products.Add (key, Constructor<T>);
	}

	public TBase Produce (TKey key)
	{
		return _Products [key] ();
	}

	static TBase Constructor<T> () where T : TBase, new()
	{
		return new T ();
	}
}

class BaseClass
{
}

class ChildClass1 : BaseClass
{
}

class ChildClass2 : BaseClass
{
}

class TestClass
{
	public static int Main ()
	{
		var factory = new Factory<byte, BaseClass> ();
		factory.Register<ChildClass1> (1);
		factory.Register<ChildClass2> (2);

		if (factory.Produce (1).GetType () != typeof (ChildClass1))
			return 1;

		if (factory.Produce (2).GetType () != typeof (ChildClass2))
			return 2;

		return 0;
	}
}