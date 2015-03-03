// CS1570: XML documentation comment on `Testing.Test2' is not well-formed XML markup (The 'summary' start tag on line 1 position 2 does not match the end tag of 'incorrect'. Line 3, position 3.)
// Line: 12
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

using System;

namespace Testing
{
	/// <summary>
	/// Incorrect comment markup.
	/// </incorrect>
	public class Test2
	{
	}
}

