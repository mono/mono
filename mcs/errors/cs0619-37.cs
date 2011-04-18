// CS0619-37: `ObsoleteEnum.value_B' is obsolete: `It's obsolete'
// Line: 16

using System;

enum ObsoleteEnum
{
    value_A,
    [Obsolete("It's obsolete", true)]
    value_B
}

enum E2
{
    aa = ObsoleteEnum.value_A,
    bb = ObsoleteEnum.value_B
}
