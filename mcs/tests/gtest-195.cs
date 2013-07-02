using System;
using System.Collections.Generic;

public class OrderedMultiDictionary<T,U>
{
        private RedBlackTree<KeyValuePair<T,U>> tree;

        private void EnumerateKeys (RedBlackTree<KeyValuePair<T,U>>.RangeTester rangeTester)
        {
                tree.EnumerateRange (rangeTester);
	}
}

internal class RedBlackTree<S>
{
        public delegate int RangeTester (S item);

        public void EnumerateRange (RangeTester rangeTester)
	{
	}
}

class X
{
	public static void Main ()
	{ }
}
