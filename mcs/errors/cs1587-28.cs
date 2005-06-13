// cs1587.cs: XML comment is placed on an invalid language element which can not accept it.
// Line: 16
// Compiler options: -doc:dummy.xml -warnaserror -warn:2

using System;

namespace TopNS
{
	abstract class AFoo
	{
		internal abstract int Prop { get; }
	}

	class Foo : AFoo
	{
		sealed /** is "sealed" checked? */ internal override int Prop {
			get { return 0; }
		}
	}
}
