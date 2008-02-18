

using System;
using System.Collections.Generic;

public static class Foo
{
        public static IEnumerable<T> Reverse<T> (this IEnumerable<T> self)
        {
                return self;
        }

        public static void Main ()
        {
                int [] data = {0, 1, 2};

                var rev = data.Reverse ();
        }
}
