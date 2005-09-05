// gtest-198.cs : bug #75957
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary2
{
        public class List1<T> : List<T>
        { }

        public class List2<T>
        {
                private List1<T> _List = new List1<T>();
                public void AddItem(T item)
                {
                        _List.Add(item);
                }
        }

	class Foo
	{
		public static void Main () {}
	}
}
 

