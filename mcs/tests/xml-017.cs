// Compiler options: -doc:xml-017.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// comment on method without parameter
		public static void Foo ()
		{
		}

		/// here is a documentation with parameters (and has same name)
		public static void Foo (long l, Test t, System.Collections.ArrayList al)
		{
		}

		/// here is a documentation with parameters (and has same name)
		public static void Foo (params string [] param)
		{
		}
	}
}

