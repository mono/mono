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

		public static implicit operator String (string s1) 
		{
			if(s1==null) return null;
			return new String(s1);
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
		public static int Main ()
		{
			String a = "a";
			int i=1;
			a+=i;
			string s = i + a;
			
			Console.WriteLine (s);
			if (s != "1Test.String")
				return 1;
				
			return 0;
		}
	}
}
