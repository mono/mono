using System.Runtime.CompilerServices;
using System;

class TestCallerLineNumber
{
    static void Test ([CallerLineNumber] object  line = null)
    {
    }

    static void Test ([CallerLineNumber] decimal line = 1)
    {
    }

    static void Test ([CallerLineNumber] double  line = 1)
    {
    }

    static void Test ([CallerLineNumber] float   line = 1)
    {
    }

    static void Test ([CallerLineNumber] int     line = 1)
    {
    }

    static void Test ([CallerLineNumber] uint    line = uint.MaxValue)
    {
    }

    static void Test ([CallerLineNumber] long    line = 1)
    {
    }

    static void Test ([CallerLineNumber] ulong   line = 1)
    {
    }

    static void Test ([CallerLineNumber] decimal? line = 1)
    {
    }

    static void Test ([CallerLineNumber] double?  line = 1)
    {
    }

    static void Test ([CallerLineNumber] float?   line = 1)
    {
    }

    static void Test ([CallerLineNumber] int?    line = 1)
    {
    }

    static void Test ([CallerLineNumber] uint?    line = uint.MaxValue)
    {
    }

    static void Test ([CallerLineNumber] long?    line = 1)
    {
    }

    static void Test ([CallerLineNumber] ulong?   line = 1)
    {
    }
}

class D
{
    public static void Main ()
    {
    }
}