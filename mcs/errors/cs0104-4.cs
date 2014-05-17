// CS0104: `XAttribute' is an ambiguous reference between `A.XAttribute' and `B.XAttribute'
// Line: 21

using System;

namespace A
{
	class XAttribute : Attribute { }
}

namespace B
{
	class XAttribute : Attribute { }
}

namespace C
{
	using A;
	using B;

	[X]
	class Test 
	{
	}
}
