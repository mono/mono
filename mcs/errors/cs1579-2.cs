// cs1579.cs: foreach statement cannot operate on variables of type 'Foo' because 'Foo' does not contain a public definition for 'GetEnumerator'
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