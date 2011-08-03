// CS1570: XML documentation comment on `Testing.EnumTest2' is not well-formed XML markup ('summary' is expected  Line 3, position 4.)
// Line: 12
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	/// <summary>
	/// comment for enum type
	/// </incorrect>
	enum EnumTest2
	{
		Foo,
		Bar,
	}
}

