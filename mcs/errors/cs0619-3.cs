// cs0619.cs: 'ObsoleteIface ' is obsolete: 'Do not use it'
// Line: 13

using System;

[Obsolete("Yeah, is obsolete", true)]
enum ObsoleteEnum
{
}

interface Ex
{
	ObsoleteEnum Foo ();
}