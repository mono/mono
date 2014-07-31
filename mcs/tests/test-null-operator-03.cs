using System;

class C
{
    int field;

    int Test1 ()
    {
        var x = this?.field;
        if (x == null)
            return 1;

        // TODO: Should it really be of int? type

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