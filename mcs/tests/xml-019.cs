// Compiler options: -doc:xml-019.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// <summary>
		/// comment for unary operator
		/// </summary>
		public static bool operator ! (Test t)
		{
			return false;
		}

		/// <summary>
		/// comment for binary operator
		/// </summary>
		public static int operator + (Test t, int b)
		{
			return b;
		}
	}
}

