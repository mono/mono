// CS0619: `ObsoleteClass' is obsolete: `Is obsolete'
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