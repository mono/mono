// CS1570: XML documentation comment on `Testing.MyDelegate2' is not well-formed XML markup (The 'summary' start tag on line 1 position 2 does not match the end tag of 'incorrect'. Line 3, position 3.)
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

