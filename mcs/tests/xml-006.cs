// Compiler options: -doc:xml-006.xml
using System;

namespace Testing
{
	/// <summary>
	/// comment for enum type
	/// </summary>
	public enum EnumTest
	{
		Foo,
		Bar,
	}

	/// <summary>
	/// comment for enum type
	/// </incorrect>
	public enum EnumTest2
	{
		Foo,
		Bar,
	}

	/**
	<summary>
	Java style comment for enum type
	</summary>
	*/
	public enum EnumTest3
	{
		Foo,
		Bar,
	}

	public class Test
	{
		public static void Main ()
		{
		}
	}
}

