// cs1587-16.cs: XML comment is not placed on a valid language element
// Line: 15
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;

namespace TopNS
{
	class Foo
	{
		string this [string bar] {
			/// incorrect
			get { return ""; }
			set { }
		}
	}
}
