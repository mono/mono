// CS1570: XML documentation comment on `Testing.Test.PublicField2' is not well-formed XML markup (The 'summary' start tag on line 1 position 2 does not match the end tag of 'invalid'. Line 3, position 3.)
// Line: 19
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	public class Test
	{
		/// <summary>
		/// comment for public field
		/// </invalid>
		public string PublicField2;
	}
}

