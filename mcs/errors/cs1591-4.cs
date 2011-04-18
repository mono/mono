// CS1591: Missing XML comment for publicly visible type or member `Testing.Foo.Foo'
// Line: 12
// Compiler options: -doc:dummy.xml -warnaserror -warn:4

using System;

namespace Testing
{
	/// comment is here.
	public enum Foo
	{
		Foo,
		/// required for all enum members
		Bar
	}
}
