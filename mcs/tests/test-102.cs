using System;
using System.Reflection;

[assembly: AssemblyTitle ("Foo")]
[assembly: AssemblyVersion ("1.0.2")]

namespace N1 {
		
	[AttributeUsage (AttributeTargets.All)]
	public class MineAttribute : Attribute {

		string name;
		
		public MineAttribute (string s)
		{
			name = s;
		}
	}

	interface A {
		[Mine ("hello")]
		void Hello ();
	}	

	public class Foo {	
		
		int i;
		
		[Mine ("Foo")]
		[return: Mine ("Bar")]	
		public static int Main ()
		{
			return 0;
		}
	}
}
