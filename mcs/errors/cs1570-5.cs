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

