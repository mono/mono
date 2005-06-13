// cs1570.cs: XML comment on 'T:Testing.MyDelegate2' has non-well-formed XML (unmatched closing element: expected summary but found incorrect  Line 3, position 12.).
// Line: 17
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	/// <summary>
	/// comment for delegate type
	/// </summary>
	public delegate void MyDelegate (object o, EventArgs e);

	/// <summary>
	/// comment for delegate type
	/// </incorrect>
	public delegate void MyDelegate2 (object o, EventArgs e);

	/**
	<summary>
	Javadoc comment for delegate type
	</summary>
	*/
	public delegate void MyDelegate3 (object o, EventArgs e);

	public class Test
	{
		public static void Main ()
		{
		}
	}
}

