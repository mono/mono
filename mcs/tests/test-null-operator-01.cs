using System;

struct S
{
    public int Prop { get; set; }
}

interface I
{
    int Method ();
}

class CI : I
{
    public int Method ()
    {
        return 33;
    }

    public int Prop { get; set; }
}

class C
{
    static int prop_calls;
    static string Prop {
        get {
            ++prop_calls;
            return null;
        }
    }

    static int TestArray ()
    {
        int[] k = null;
        var t1 = k?.ToString ();
        if (t1 != null)
            return 1;

        var t2 = k?.GetLength (0);
        if (t2 != null)
            return 2;

        var t3 = k?.Length;
        if (t3 != null)
            return 3;

        var t4 = k?.GetLength (0).ToString () ?? "N";
        if (t4 != "N")
            return 4;

        var t5 = k?.Length.ToString () ?? "N";
        if (t5 != "N")
            return 5;            

        k = new int[] { 3 };
        var t11 = k?.ToString ();
        if (t11.GetType () != typeof (string))
            return 10;

        var t12 = k?.GetLength (0);
        if (t12.GetType () != typeof (int))
            return 11;

        var t13 = k?.Length;
        if (t13.GetType () != typeof (int))
            return 12;

        return 0;
    }

    static int TestReferenceType ()
    {
        string s = null;
        var t1 = s?.Split ();
        if (t1 != null)
            return 1;

        var t2 = s?.Length;
        if (t2 != null)
            return 2;

        var t3 = Prop?.Length;
        if (t3 != null)
            return 3;
        if (prop_calls != 1)
            return 4;

        var t4 = Prop?.Split ();
        if (t4 != null)
            return 5;
        if (prop_calls != 2)
            return 6;

        return 0;
    }

    static int TestGeneric<T> (T t) where T : class, I
    {
        var t1 = t?.Method ();
        if (t1 != null)
            return 1;

        T[] at = null;
        var t2 = at?.Length;
        if (t2 != null)
            return 2;

        return 0;
    }

    static int TestNullable ()
    {
        int? i = 4;
        var m = i?.CompareTo (3);
        if (m.GetType () != typeof (int))
            return 1;

        if (m != 1)
            return 2;

        DateTime? dt = null;
        dt?.ToString ();
        if (dt?.ToString () != null)
            return 3;

        byte? b = 0;
        if (b?.ToString () != "0")
            return 4;

        S? s = null;
        var p1 = s?.Prop;
        if (p1 != null)
            return 5;

        return 0;
    }

    static int Main ()
    {
        int res;
        res = TestNullable ();
        if (res != 0)
            return 100 + res;

        res = TestArray ();
        if (res != 0)
            return 200 + res;

        res = TestReferenceType ();
        if (res != 0)
            return 300 + res;

        CI ci = null;
        res = TestGeneric<CI> (ci);
        if (res != 0)
            return 400 + res;

        Console.WriteLine ("ok");
        return 0;
    }
}