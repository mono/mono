// CS0579: The attribute `ConditionalAttributeTesting.SomeAttribute' cannot be applied multiple times
// Line: 12

using System;
using System.Diagnostics;

namespace ConditionalAttributeTesting
{
	class MainClass
	{
		[Some ("Test")]
		[Some ("Test2")]
		public static void Test ()
		{
		}
	}

	[AttributeUsage (AttributeTargets.All)]
	[Conditional ("NOT_DEFINED")]
	public sealed class SomeAttribute : Attribute
	{
		public SomeAttribute (string someText)
		{
		}
	}
}