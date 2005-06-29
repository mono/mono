// cs1591-18.cs: Missing XML comment for publicly visible type or member `Testing.Test.InternalStruct'
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
		public struct InternalStruct
		{
		}
	}
}
