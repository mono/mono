// Compiler options: -r:test-738-lib.dll

using System;

namespace TestNamespace
{
	public class ResumableInputStream
	{
		public ResumableInputStream()
		{
			stream.Dispose();
		}

		private NonClosingStream stream;
		
		public static void Main ()
		{
		}
	}
}

