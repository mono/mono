// Compiler options: -doc:xml-014.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// <summary>
		/// comment for private property
		/// </summary>
		private string PrivateProperty {
			get { return null; }
			set { }
		}
	}
}

