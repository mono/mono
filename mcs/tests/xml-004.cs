// Compiler options: -doc:xml-004.xml
using System;

namespace Testing
{
	/// <summary> 
	/// comment for struct
	/// </summary>
	public struct StructTest
	{
	}

	/// <summary> 
	/// incorrect markup comment for struct
	/// </incorrect>
	public struct StructTest2
	{
	}

	/**
		<summary>
		Java style commet
		</summary>
	*/
	public struct StructTest3
	{
	}

	public class Test
	{
		public static void Main ()
		{
		}
	}
}

