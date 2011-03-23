// CS1570: XML documentation comment on `Testing.MyDelegate2' is not well-formed XML markup ('summary' is expected  Line 3, position 4.)
// Line: 12
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	/// <summary>
	/// comment for delegate type
	/// </incorrect>
	public delegate void MyDelegate2 (object o, EventArgs e);
}

