// Compiler options: -target:library

using System;

namespace Test2
{
	static class Extensions {
		public static bool IsNullable (this Type self)
		{
			return true;
		}
	}
}