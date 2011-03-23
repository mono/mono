// CS1570: XML documentation comment on `Testing.Test2' is not well-formed XML markup ('summary' is expected  Line 3, position 4.)
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

