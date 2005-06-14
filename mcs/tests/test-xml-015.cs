// Compiler options: -doc:xml-015.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		private string PrivateProperty {
			get { return null; }
			/// <summary>
			/// comment for private property setter - no effect
			/// </summary>
			set { }
		}

	}
}

