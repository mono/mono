// Compiler options: -doc:dummy.xml -warnaserror -warn:1
using System;

namespace Testing
{
	/// <summary>
	/// comment on class
	/// </summary>
	public class Test
	{
		public static void Main ()
		{
		}
	}

	/// <summary>
	/// Incorrect comment markup.
	/// </incorrect>
	public class Test2
	{
	}

	/**
		<summary>
		another Java-style documentation style
		</summary>
	*/
	public class Test3
	{
	}
}

