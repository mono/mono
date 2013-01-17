namespace A
{
	public interface IExtensible<T>
	{
		void AddAll<U> (U u)
			where U : T;
	}

	public class ArrayList<T> : IExtensible<T>
	{
		void IExtensible<T>.AddAll<U> (U u)
		{
			InsertAll (u);
		}

		void InsertAll (T t)
		{ }
	}
}

namespace B
{
	public interface IExtensible<S,T>
	{
		void AddAll<U> (U t)
			where U : S;
	}

	public class ArrayList<X,Y> : IExtensible<Y,X>
	{
		public void AddAll<Z> (Z z)
			where Z : Y
		{
			InsertAll (z);
		}

		void InsertAll (Y y)
		{ }
	}
}

namespace C
{
	public interface IExtensible<S>
	{
		void AddAll<T> (T t)
			where T : S;
	}

	public class Foo<U>
	{ }

	public class ArrayList<X> : IExtensible<Foo<X>>
	{
		public void AddAll<Y> (Y y)
			where Y : Foo<X>
		{
			InsertAll (y);
		}

		void InsertAll (Foo<X> foo)
		{ }
	}
}

class X
{
	public static void Main ()
	{ }
}
