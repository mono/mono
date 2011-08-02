// Compiler options: -doc:xml-053.xml

/// <summary>Outer`2</summary>
public class Outer<T, U> {
	/// <summary>Outer`2.CopyTo(`0[],System.Int32)</summary>
	public void CopyTo(T[] array, int n)
	{
	}

	/// <summary>M:Outer`2.CopyTo(`0[,,],System.Int32)</summary>
	public void CopyTo(T[,,] array, int n)
	{
	}

	/// <summary>Outer`2.CopyTo(`0[,,][,][])</summary>
	public void CopyTo(T[][,][,,] array)
	{
	}

	/// <summary>Outer`2.CopyTo(System.Collections.Generic.KeyValuePair{`0,System.Collections.Generic.List{`1}}[],System.Int32)</summary>
	public void CopyTo(System.Collections.Generic.KeyValuePair<T,System.Collections.Generic.List<U>>[] array, int n)
	{
	}

	/// <summary>Outer`2.CopyTo``2(System.Collections.Generic.KeyValuePair{``0,System.Collections.Generic.List{``1}}[],System.Int32)</summary>
	public void CopyTo<W,X>(System.Collections.Generic.KeyValuePair<W,System.Collections.Generic.List<X>>[] array, int n)
	{
	}

	/// <summary>Outer`2.CopyTo``1(System.Collections.Generic.KeyValuePair{`1,System.Collections.Generic.List{``0}}[],System.Int32)</summary>
	public void CopyTo<V>(System.Collections.Generic.KeyValuePair<U,System.Collections.Generic.List<V>>[] array, int n)
	{
	}

	/// <summary>Outer`2.Foo``1(``0[])</summary>
	public void Foo<T>(T[] array)
	{
	}

	/// <summary>Outer`2.Foo``1(``0[],`0)</summary>
	public void Foo<S>(S[] array, T value)
	{
	}

	/// <summary>Outer`2:Inner`1</summary>
	public class Inner<V> {
		/// <summary>Outer`2.Inner`1.Bar(`0@,`1,`2)</summary>
		public static void Bar(ref T t, U u, V v)
		{
		}
	}
}

/// <summary>T:Util</summary>
public class Util {
	/// <summary>Util.Convert``2(``1[])</summary>
	public static TResult Convert<TResult,TSource>(TSource[] input)
		where TResult : TSource
	{
		return default (TResult);
	}
}

interface IFoo<T>
{
	void Foo ();
}

class C : IFoo<int>
{
	/// <summary>Test</summary>
	void IFoo<int>.Foo ()
	{
	}
}

class Test {
	public static void Main ()
	{
	}
}

