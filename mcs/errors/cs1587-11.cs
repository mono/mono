// Compiler options: -doc:dummy.xml -warnaserror -warn:2
using System;

namespace TopNS
{
	namespace ChildNS {
		[Flags]
		/// comment after attribute
		enum Enum2 {
		}
	}
}
