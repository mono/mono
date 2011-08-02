// CS0266: Cannot implicitly convert type `int' to `X.E'. An explicit conversion exists (are you missing a cast?)
// Line : 9

class X {
        enum E { }
        
        static void Main ()
        {
                const E e = 1 - 1;
        }
}



