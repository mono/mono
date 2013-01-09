using System;
using System.Reflection;
using System.Diagnostics;

namespace ConditionalAttributeTesting
{
	class MainClass
	{
		public static int Main ()
		{
			return HelloWorld ();
		}

		[Some ("Test")]
		public static int HelloWorld ()
		{
			var methodInfo = MethodBase.GetCurrentMethod ();
			SomeAttribute someAttribute = Attribute.GetCustomAttribute (methodInfo, typeof (SomeAttribute)) as SomeAttribute;
			if (someAttribute != null) {
				return 1;
			}

			return 0;
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