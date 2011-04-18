// CS0202: foreach statement requires that the return type `Foo.E[]' of `Foo.P.GetEnumerator()' must have a suitable public MoveNext method and public Current property
// Line: 18

public class Foo
{
        public class E {}
            
        public class P
        {
            public E[] GetEnumerator ()
            {
                return null;
            }
        }
       
        public static void Main ()
        {
            P o = new P ();
            foreach (P p in o)
            {
            }
        }
}
