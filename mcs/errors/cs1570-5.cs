// cs1570-5.cs: XML comment on `T:Testing.EnumTest2' has non-well-formed XML ('summary' is expected  Line 3, position 4.)
// Line: 17
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

