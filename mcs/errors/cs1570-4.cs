// cs1570-4.cs: XML comment on `T:Testing.InterfaceTest2' has non-well-formed XML (unmatched closing element: expected summary but found incorrect  Line 3, position 12.)
// Line: 19
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	/// <summary>
	/// comment for interface
	/// </summary>
	public interface InterfaceTest
	{
	}

	/// <summary>
	/// incorrect markup comment for interface
	/// </incorrect>
	public interface InterfaceTest2
	{
	}

	/**
		<summary>
		Java style comment for interface
		</summary>
	*/
	public interface InterfaceTest3
	{
	}

	public class Test
	{
		public static void Main ()
		{
		}
	}
}

