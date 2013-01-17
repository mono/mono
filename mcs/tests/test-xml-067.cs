// Compiler options: support-xml-067.cs -doc:xml-067.xml -warnaserror

// Partial types can have documentation on one part only

using System;

namespace Testing
{
	/// <summary>
	/// description for class Test
	/// </summary>
	public partial class Test
	{
		/// test
		public Test ()
		{
		}
	}

	public partial class Test
	{
		/// test 2
		public void Foo ()
		{
		}

		static void Main ()
		{
		}
	}
}
