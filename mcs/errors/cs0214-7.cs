// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 6
// Compiler options: -unsafe

class Test
{
        public void Main ()
        {
                byte* arr = stackalloc byte [4];
        }
}

