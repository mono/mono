// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 9
// Compiler options: -unsafe

public class Test
{
        public void Foo ()
        {
                Foo (null);
        }

        public static unsafe void Foo (int* buf) { }
}


