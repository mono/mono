// CS1510: A ref or out argument must be an assignable variable
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
