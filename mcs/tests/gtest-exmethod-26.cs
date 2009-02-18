// Compiler options: -r:gtest-exmethod-26-lib.dll

using System;
using Test2;
using test;

namespace test
{
	internal static class TypeExtensions
	{
		public static bool IsNullable (this Type t)
		{
			return true;
		}
	}
}
namespace testmono
{
	class MainClass
	{
		public static void Main ()
		{
			string s = "";
			if (s.GetType ().IsNullable ()) {
				Console.WriteLine ("aaa");
			}
		}
	}
}
