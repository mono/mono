// error CS1510: An lvalue is required as an argument to out or ref
// Line: 19
// this is bug #70402

using System;
 
class T {
 
        enum A { a, b }
 
        static void Convert (out A a)
        {
                a = A.a;
        }
 
        static void Main ()
        {
                int a = 0;
                Convert (out (A) a);
        }
}
