// CS0619: `ObsoleteEnum' is obsolete: `Yeah, is obsolete'
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