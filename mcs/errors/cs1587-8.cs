// CS1587: XML comment is not placed on a valid language element
// Line: 10
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;

namespace TopNS
{
	/// more invalid comment on namespace; inside namespace
	namespace ChildNS {
		class Test {
		}
	}

}
