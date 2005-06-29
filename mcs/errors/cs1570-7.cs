// cs1570-7.cs: XML comment on `T:Testing.Test2' has non-well-formed XML (a name did not start with a legal character 54 (6)  Line 1, position 3.)
// Line: 18
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	/// comment without markup on class - it is allowed
	public class Test
	{
		public static void Main ()
		{
		}
	}

	/// <6roken> broken markup
	public class Test2
	{
	}

	/// <dont-forget-close-tag>
	public class Test3
	{
	}
}

