// cs1518.cs : Attribute cannot be applied to namespaces. Expected class, delegate, enum, interface, or struct
// Line : 14

using System;

[assembly: CLSCompliant (false)]

namespace N
{
}

[assembly: Obsolete]

namespace M
{
}

public class C {
    public static void Main () {}
}