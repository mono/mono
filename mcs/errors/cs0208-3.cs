// cs0202.cs: The call to GetEnumerator must return a class or a struct, not 'Foo.P*'
// Line: 18
// Compiler options: -unsafe

public unsafe class Foo
{
        public class P
        {
            public P* GetEnumerator ()
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
