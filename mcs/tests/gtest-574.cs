using System;
using System.Collections.Generic;

public class TestClass<T1> : SequencedBase<T1>, IIndexedSorted<T1>
{
	void Test ()
	{
		TestClass<T1> tt = null;
		tt.Foo (this);
	}

	public void Foo<U> (IEnumerable<U> items)
	{
	}

	public class Nested : ICloneable
	{
		public object Clone ()
		{
			return null;
		}
	}
}

public abstract class SequencedBase<T2> : DirectedCollectionBase<T2>, IDirectedCollectionValue<T2>
{
	public T2 Field;
}

public abstract class DirectedCollectionBase<T3> : CollectionBase<T3>, IDirectedCollectionValue<T3>
{
	IEnumerator<T3> IEnumerable<T3>.GetEnumerator ()
	{
		return null;
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
	{
		return null;
	}
}

public abstract class CollectionBase<T4> : CollectionValueBase<T4>
{
}

public abstract class CollectionValueBase<T5> : EnumerableBase<T5>, ICollectionValue<T5>
{
}

public abstract class EnumerableBase<T6> : IEnumerable<T6>
{
	IEnumerator<T6> IEnumerable<T6>.GetEnumerator ()
	{
		return null;
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
	{
		return null;
	}
}

public interface IDirectedCollectionValue<T7> : ICollectionValue<T7>, IDirectedEnumerable<T7>
{
}

public interface ICollectionValue<T8> : IEnumerable<T8>
{
}

public interface IIndexedSorted<T9> : ISorted<T9>, IIndexed<T9>
{
}

public interface ISorted<T10> : ISequenced<T10>
{
}

public interface ISequenced<T11> : IDirectedCollectionValue<T11>
{
}

public interface IDirectedEnumerable<T12> : IEnumerable<T12>
{
}

public interface IIndexed<T13> : ISequenced<T13>
{
}

class C
{
	public static void Main ()
	{
		var c = new TestClass<string> ();
	}
}