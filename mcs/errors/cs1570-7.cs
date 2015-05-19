// CS1570: XML documentation comment on `Testing.Test2' is not well-formed XML markup (Name cannot begin with the '6' character, hexadecimal value 0x36. Line 1, position 2.)
// Line: 9
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	/// <6roken> broken markup
	public class Test2
	{
	}
}

