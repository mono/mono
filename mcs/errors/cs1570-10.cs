// CS1570: XML documentation comment on `Testing.Test.PrivateField2' is not well-formed XML markup ('summary' is expected  Line 3, position 4.)
// Line: 23
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

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

