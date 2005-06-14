// Compiler options: -doc:xml-018.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// <summary>
		/// comment for indexer
		/// </summary>
		public string this [int i] {
			get { return null; }
		}

		/// <summary>
		/// comment for indexer
		/// </summary>
		public string this [string s] {
			get { return null; }
		}

		/// <summary>
		/// comment for indexer wit multiple parameters
		/// </summary>
		public string this [int i, Test t] {
			get { return null; }
		}

	}
}

