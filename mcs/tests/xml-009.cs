// Compiler options: -doc:xml-009.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
			/// inside method - not allowed.
		}
	}

	public class Test2
	{
		/// no target
	}

	public class Test3
	{
	}
	/// no target case 2.
}

