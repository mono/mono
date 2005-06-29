// cs1570-9.cs: XML comment on `F:Testing.Test.PublicField2' has non-well-formed XML (unmatched closing element: expected summary but found invalid  Line 3, position 10.)
// Line: 19
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	public class Test
	{
		/// <summary>
		/// comment for public field
		/// </summary>
		public string PublicField;

		/// <summary>
		/// comment for public field
		/// </invalid>
		public string PublicField2;

		/**
		 <summary>
		 Javadoc comment for public field
		 </summary>
		*/
		public string PublicField3;

		public static void Main ()
		{
		}
	}
}

