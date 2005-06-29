// cs1570-2.cs: XML comment on `T:Testing.Test2' has non-well-formed XML (unmatched closing element: expected summary but found incorrect  Line 3, position 12.)
// Line: 22
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	/// <summary>
	/// comment on class
	/// </summary>
	public class Test
	{
		public static void Main ()
		{
		}
	}

	/// <summary>
	/// Incorrect comment markup.
	/// </incorrect>
	public class Test2
	{
	}

	/**
		<summary>
		another Java-style documentation style
		</summary>
	*/
	public class Test3
	{
	}
}

