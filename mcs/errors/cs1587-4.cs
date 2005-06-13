// cs1587.cs: XML comment is placed on an invalid language element which can not accept it.
// Line: 8
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;

/// invalid comment placed on namespace.
namespace TopNS
{
	class Foo
	{
	}
}
