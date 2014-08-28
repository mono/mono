using System;

class C
{
    int field;

    int Test1 ()
    {
        var x = this?.field;
        if (x == null)
            return 1;

        var x2 = "abc"?.GetHashCode();
        if (x2 == null)
            return 2;

        return 0;
    }

    static int Main ()
    {
        var c = new C ();
        c.Test1 ();

        const C c2 = null;
        var res = c2?.field;
        if (res != null)
            return 1;

    	Console.WriteLine ("ok");
        return 0;
    }
}