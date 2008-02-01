// Compiler options: -t:library

using System;

namespace Testy
{
	public static class TestExtensions
	{
		public static string MyFormat (this Object junk,
						  string fmt, params object [] args)
		{
			return String.Format (fmt, args);
		}
	}
}
