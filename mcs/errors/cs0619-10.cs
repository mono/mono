// cs0619.cs: 'ObsoleteClass' is obsolete: 'Do not use it'
// Line: 10

using System;

[Obsolete("Do not use it.", true)]
class ObsoleteClass {
}

class C: ObsoleteClass
{
}