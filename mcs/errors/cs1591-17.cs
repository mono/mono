// CS1591: Missing XML comment for publicly visible type or member `Testing.Test.InternalClass'
// Line: 14
// Compiler options: -doc:dummy.xml -warnaserror -warn:4

using System;

namespace Testing
{
	/// <summary>
	/// description for class Test
	/// </summary>
	public class Test
	{
		protected class InternalClass
		{
		}
	}
}
