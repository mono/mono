// cs0619.cs: 'ObsoleteIface ' is obsolete: 'Do not use it'
// Line: 13

using System;

[Obsolete("Is obsolete", true)]
class ObsoleteClass
{
}

interface Ex
{
	void Foo (ObsoleteClass o1, ObsoleteClass o2);
}