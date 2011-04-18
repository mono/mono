// CS0619-38: `ObsoleteEnum' is obsolete: `ooo'
// Line: 14

using System;

[Obsolete("ooo", true)]
enum ObsoleteEnum
{
    value_A
}

class C
{
    static ObsoleteEnum oe = ObsoleteEnum.value_A;
}
