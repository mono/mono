//
// tuple names attribute decoration
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

interface I<T>
{
}

class B<T>
{
}

class C // : B<(int a, int b)> // TODO: I<(a, b)
{
    public C((int a, int b) d)
    {
    }

    public (int a, int b) a;
    public (int, (int a, int b)) c;

    public (int a, int b) Prop { set; get; }
    public (int a, int b) Prop2 { set { } }

    public (int a, int b)? this[(int a, int b) d] { set { } get { return null; } }

    public (int a, int b)? Method(ref (int a, int b) d)
    {
        return null;
    }

    public (int a, int b)[] t;
    public (int a, int b)[,] t2;
    // TODO:    public Func<(int a, int b), int, (int c, int d)[]> v;
    //    public I<(int a, int b)>[] iface;
    // TODO:    public Action<(long, (long u, long))[], object, (int a, int b)> d2;
    public (((int aa1, int aa2) a1, (int, int) a2) x1, ((int cc1, int cc2) b1, (int dd1, int dd2) b2) x2) d3;
}

delegate (int a, int b) Del((int a, int b) d);

class Test
{
    public static int Main()
    {
        Type t = typeof(C);
        Type ca = typeof(TupleElementNamesAttribute);
        TupleElementNamesAttribute da;

        if (t.GetMember("a")[0].GetCustomAttributes(ca, false).Length != 1)
            return 1;

        if (t.GetMember("c")[0].GetCustomAttributes(ca, false).Length != 1)
            return 3;

        if (t.GetMember("Prop")[0].GetCustomAttributes(ca, false).Length != 1)
            return 4;

        if (t.GetMember("get_Prop")[0].GetCustomAttributes(ca, false).Length != 0)
            return 5;

        if (t.GetMethod("get_Prop").ReturnParameter.GetCustomAttributes(ca, false).Length != 1)
            return 6;

        if (t.GetMember("set_Prop")[0].GetCustomAttributes(ca, false).Length != 0)
            return 7;

        if (t.GetMethod("set_Prop").ReturnParameter.GetCustomAttributes(ca, false).Length != 0)
            return 8;

        if (t.GetMethod("set_Prop").GetParameters()[0].GetCustomAttributes(ca, false).Length != 1)
            return 9;

        if (t.GetMember("Prop2")[0].GetCustomAttributes(ca, false).Length != 1)
            return 10;

        if (t.GetMember("set_Prop2")[0].GetCustomAttributes(ca, false).Length != 0)
            return 11;

        if (t.GetMember("Item")[0].GetCustomAttributes(ca, false).Length != 1)
            return 12;

        if (t.GetMethod("get_Item").ReturnParameter.GetCustomAttributes(ca, false).Length != 1)
            return 13;

        if (t.GetMethod("get_Item").GetParameters()[0].GetCustomAttributes(ca, false).Length != 1)
            return 14;

        if (t.GetMethod("set_Item").ReturnParameter.GetCustomAttributes(ca, false).Length != 0)
            return 15;

        if (t.GetMethod("set_Item").GetParameters()[0].GetCustomAttributes(ca, false).Length != 1)
            return 16;

        if (t.GetMethod("set_Item").GetParameters()[1].GetCustomAttributes(ca, false).Length != 1)
            return 17;

        if (t.GetMember("Method")[0].GetCustomAttributes(ca, false).Length != 0)
            return 18;

        var res = t.GetMethod("Method").GetParameters()[0].GetCustomAttributes(ca, false);
        if (res.Length != 1)
            return 19;

        da = res[0] as TupleElementNamesAttribute;
        if (da == null)
            return 190;
        if (!da.TransformNames.SequenceEqual(new string[] { "a", "b" }))
            return 191;

        if (t.GetConstructors()[0].GetParameters()[0].GetCustomAttributes(ca, false).Length != 1)
            return 20;

        if (t.GetConstructors()[0].GetCustomAttributes(ca, false).Length != 0)
            return 21;

        //        if (t.GetCustomAttributes(ca, false).Length != 1)
        //            return 22;

        // Transformations
        da = t.GetMember("t")[0].GetCustomAttributes(ca, false)[0] as TupleElementNamesAttribute;
        if (da == null)
            return 40;

        if (!da.TransformNames.SequenceEqual(new string[] { "a", "b" }))
            return 41;

        da = t.GetMember("t2")[0].GetCustomAttributes(ca, false)[0] as TupleElementNamesAttribute;
        if (da == null)
            return 42;

        if (!da.TransformNames.SequenceEqual(new string[] { "a", "b" }))
            return 43;

        //da = t.GetMember("v")[0].GetCustomAttributes(ca, false)[0] as TupleElementNamesAttribute;
        //if (da == null)
        //    return 44;

        //if (!da.TransformNames.SequenceEqual(new string[] { "a", "b", "c", "d" }))
        //    return 45;

        //da = t.GetMember("iface")[0].GetCustomAttributes(ca, false)[0] as TupleElementNamesAttribute;
        //if (da == null)
        //    return 46;

        //if (!da.TransformNames.SequenceEqual(new string[] { "a", "b" }))
        //    return 47;

        //da = t.GetMember("d2")[0].GetCustomAttributes(ca, false)[0] as TupleElementNamesAttribute;
        //if (da == null)
        //    return 48;
        //if (!da.TransformNames.SequenceEqual(new string[] { null, null, "u", null, "a", "b" }))
        //    return 49;

        da = t.GetMember("d3")[0].GetCustomAttributes(ca, false)[0] as TupleElementNamesAttribute;
        if (da == null)
            return 50;
        if (!da.TransformNames.SequenceEqual(new string[] { "x1", "x2", "a1", "a2", "aa1", "aa2", null, null, "b1", "b2", "cc1", "cc2", "dd1", "dd2" }))
            return 51;

        t = typeof(Del);

        if (t.GetMember("Invoke")[0].GetCustomAttributes(ca, false).Length != 0)
            return 100;

        if (t.GetMethod("Invoke").GetParameters()[0].GetCustomAttributes(ca, false).Length != 1)
            return 101;

        if (t.GetMethod("Invoke").ReturnParameter.GetCustomAttributes(ca, false).Length != 1)
            return 102;

        Console.WriteLine("ok");
        return 0;
    }
}
