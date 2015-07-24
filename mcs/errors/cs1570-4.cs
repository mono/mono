// CS1570: XML documentation comment on `Testing.InterfaceTest2' is not well-formed XML markup (The 'summary' start tag on line 1 position 2 does not match the end tag of 'incorrect'. Line 3, position 3.)
// Line: 12
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	/// <summary>
	/// incorrect markup comment for interface
	/// </incorrect>
	public interface InterfaceTest2
	{
	}
}

