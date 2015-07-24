// CS1570: XML documentation comment on `Testing.StructTest2' is not well-formed XML markup (The 'summary' start tag on line 1 position 2 does not match the end tag of 'incorrect'. Line 3, position 3.)
// Line: 10
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

namespace Testing
{
	/// <summary> 
	/// incorrect markup comment for struct
	/// </incorrect>
	public struct StructTest2
	{
	}
}

