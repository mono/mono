// cs0214: pointers can only be used in an unsafe context
// Line: 9
// This error shows how cs214 is produced at the *call site*
//
public class Test
{
        public void Foo ()
        {
                Foo (null);
        }

        public static unsafe void Foo (int* buf) { }
}


