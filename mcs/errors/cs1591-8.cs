// CS1591: Missing XML comment for publicly visible type or member `Testing.Test.PublicProperty'
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
		public string PublicProperty {
			get { return null; }
		}
	}
}
