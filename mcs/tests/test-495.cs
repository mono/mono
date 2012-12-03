// This code must be compilable without any warning
// Compiler options: -warnaserror -warn:4

using System;

namespace plj
{
	public abstract class aClass
	{
		public static implicit operator aClass(fromClass o)
		{ 
			return null;
		}
	}
	
	public class realClass1 : aClass
	{
		public static implicit operator realClass1(fromClass o)
		{
			return null;
		}
	}
	
	public class fromClass
	{
		public static void Main () {}
	}
}