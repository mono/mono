// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `Foo.P'
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
