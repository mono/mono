// CS1591: Missing XML comment for publicly visible type or member `Testing.IFoo.Foo'
// Line: 14
// Compiler options: -doc:dummy.xml -warnaserror -warn:4

using System;

namespace Testing
{
	/// <summary>
	/// description for interface IFoo
	/// </summary>
	public interface IFoo
	{
		string Foo { get; }
	}
}
