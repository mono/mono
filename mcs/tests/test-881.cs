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
	using X = A.XAttribute;

	[X]
	class Test 
	{
		public static void Main ()
		{
		}
	}
}
