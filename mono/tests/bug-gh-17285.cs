using System;

namespace ConsoleApp2
{
    class Program
    {
        class C
        {
            public void f(int i1, int i2, int i3, int i4, int i5, int i6, int i7)
            {
                throw new Exception("exception from f()");
            }
        }

        // If this test succeeds, it should run to completion. If it fails,
        // mono's exception handler will restore an incorrect stack pointer
        // when executing the exception handler, which will cause the runtime
        // to later crash after returning to a bogus address.
        static void Main()
        {
            try
            {
                C c = new C();
                int i1 = 0, i2 = 0, i3 = 0, i4 = 0, i5 = 0, i6 = 0, i7 = 0;
                c.f(i1, i2, i3, i4, i5, i6, i7);
            }
            catch (Exception e)
            {
                Console.WriteLine("caught: " + e);
            }
        }
    }
}

