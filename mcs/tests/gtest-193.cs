using System;
using System.Collections.Generic;

public class OrderedMultiDictionary<T,U>
{
        private RedBlackTree<KeyValuePair<T,U>> tree;

        private IEnumerator<T> EnumerateKeys (RedBlackTree<KeyValuePair<T,U>>.RangeTester rangeTester)
        {
                tree.EnumerateRange (rangeTester);
		yield break;
	}
}

internal class RedBlackTree<S>
{
        public delegate int RangeTester (S item);

        public IEnumerable<S> EnumerateRange (RangeTester rangeTester)
	{
		yield break;
	}
}

class X
{
	public static void Main ()
	{ }
}
