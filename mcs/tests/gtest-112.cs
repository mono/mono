using System;

public interface IComparer<T>
{
	void Compare (T a);
}

class IC : IComparer<Foo<int>>
{
	public void Compare (Foo<int> a)
	{ }
}

public struct Foo<K>
{
	public K Value;

	public Foo (K value)
	{
		Value = value;
	}
}

public class List<T>
{
	public virtual void Sort (IComparer<T> c, T t)
	{
		Sorting.IntroSort<T> (c, t);
	}
}

public class Sorting
{
	public static void IntroSort<T> (IComparer<T> c, T t)
	{
		new Sorter<T> (c, 4, t).InsertionSort (0);
	}

	class Sorter<T>
	{
		IComparer<T> c;
		T[] a;

		public Sorter (IComparer<T> c, int size, T item)
		{
			this.c = c;
			a = new T [size];
		}

		internal void InsertionSort (int i)
		{
			T other;
			c.Compare (other = a[i]);
		}
	}
}

class X
{
	public static void Main ()
	{
		List<Foo<int>> list = new List<Foo<int>> ();
		Foo<int> foo = new Foo<int> (3);
		list.Sort (new IC (), foo);
	}
}
