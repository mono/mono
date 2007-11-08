// CS0029: Cannot implicitly convert type `string' to `Test.String'
// Line: 38

using System;

namespace Test
{
	using Text = System.Text;
	using Str = System.String;
	
	public class String
	{
		string s;
		public String(string s)
		{
			this.s=s;
		}

		public static implicit operator Str (String s1) 
		{
			if(s1==null) return null;
			return s1.ToString();
		}
	}
}

namespace TestCompiler
{
	using String=Test.String;
	
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			String a="bonjour";
			int i=1;
			Console.WriteLine(i+a);
		}
	}
}
