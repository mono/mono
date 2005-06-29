// cs0619-9.cs: `ObsoleteInterface' is obsolete: `'
// Line: 11

using System;

[Obsolete("", true)]
interface ObsoleteInterface
{
}

delegate ObsoleteInterface @delegate ();
