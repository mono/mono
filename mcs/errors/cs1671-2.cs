// CS1671: A namespace declaration cannot have modifiers or attributes
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