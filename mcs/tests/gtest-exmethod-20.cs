using System;
using System.Collections.Generic;

interface I
{
}

namespace Outer.Inner
{
	class Test {
		static void M (I list)
		{
			list.AddRange(new Test[0]);
		}
		
		public static void Main()
		{
		}
	}
}

namespace Outer {
	static class ExtensionMethods {
		public static void AddRange<T>(this I list, IEnumerable<T> items)
		{
		}
	}
}
