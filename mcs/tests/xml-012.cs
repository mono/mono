// Compiler options: -doc:xml-012.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// <summary>
		/// comment for private field
		/// </summary>
		private string PrivateField;

		/// <summary>
		/// incorrect markup comment for private field
		/// </incorrect>
		private string PrivateField2;

		/**
		<summary>
		Javadoc comment for private field
		</summary>
		*/
		private string PrivateField3;
	}
}

