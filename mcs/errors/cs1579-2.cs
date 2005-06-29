// cs1579-2.cs: foreach statement cannot operate on variables of type `Foo' because it does not contain a definition for `GetEnumerator' or is not accessible
// Line: 12

using System;
using System.Collections;

public class Test
{
        public static void Main ()
        {
                Foo f = new Foo ();
                foreach (object o in f)
                        Console.WriteLine (o);
        }
}

public class Foo
{
        internal IEnumerator GetEnumerator ()
        {
                return new ArrayList ().GetEnumerator ();
        }
}