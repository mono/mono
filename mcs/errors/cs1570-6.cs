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

