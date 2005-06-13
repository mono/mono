// cs1587.cs: XML comment is placed on an invalid language element which can not accept it.
// Line: 15
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;

namespace TopNS
{
	namespace ChildNS {
		enum Enum3 {
		}
		/// invalid here
	}
}
