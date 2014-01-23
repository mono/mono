// Compiler options: -r:test-543-lib.dll

using System;

class BetterMethod
{
    public int this[params bool[] args] { get { return 2; } }
    public string this[bool a, object b] { get { throw new NotImplementedException (); } }
}

class MainClass
{
    public int this [int expectedLength, params string[] items]
    {
        get { return 4; }
        set {
            if (expectedLength != items.Length)
                throw new ArgumentException (expectedLength + " != " + items.Length);
        }
    }

    public object this [int expectedLength, params object[] items]
    {
        get { return null; }
        set {
            if (expectedLength != items.Length)
                throw new ArgumentException (expectedLength + " != " + items.Length);
        }
    }
    
    public bool this [int expectedLength, bool isNull, params object[] items]
    {
        get { return false; }
        set {
            if (expectedLength != items.Length)
                throw new ArgumentException (expectedLength + " != " + items.Length);
        }
    }

    public static void Main(string[] args)
    {
        MainClass t = new MainClass();
        t [2, "foo", "doo"] = 2;
        t [3, new object[3]] = null;
        t [2, new int[] { 1 }, new string[] { "c", "b", "a" }] = null;
        t [0, true] = t [0, true];
        t [1, false, "foo"] = t [0, false, "foo", "doo"];
        
        ExternClass e = new ExternClass ();
        e ["a", "b", "b"] = false;
        
        BetterMethod bm = new BetterMethod ();
        Console.WriteLine (bm[true, false]);
    }
}