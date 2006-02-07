// cs1570.cs: XML comment on `T:Testing.Test2' has non-well-formed XML ('summary' is expected  Line 3, position 4.)
// Line: 22
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

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

