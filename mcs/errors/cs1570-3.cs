// cs1570-3.cs: XML comment on `T:Testing.StructTest2' has non-well-formed XML ('summary' is expected  Line 3, position 4.)
// Line: 19
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

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

