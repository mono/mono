using System;

class T {
    static int foo(int x)
    {
        return x+1;
    }

    static int bar(int x)
    {
        return foo(x);
    }

    static void Main (string[] args)
    {
        bar(5);
        foo(1);
    }
}
