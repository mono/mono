using System;
using System.Reflection;

[assembly: AssemblyTitle ("Foo")]
[assembly: AssemblyVersion ("1.0.2")]

namespace N1 {
		
	[AttributeUsage (AttributeTargets.All)]
	public class MineAttribute : Attribute {

		public string name;
		
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
			Type t = typeof (Foo);
			foreach (MemberInfo m in t.GetMembers ()){
				if (m.Name == "Main"){
					MethodInfo mb = (MethodInfo) m;

					ICustomAttributeProvider p = mb.ReturnTypeCustomAttributes;
					object [] ret_attrs = p.GetCustomAttributes (false);

					if (ret_attrs.Length != 1){
						Console.WriteLine ("Got more than one return attribute");
						return 1;
					}
					if (!(ret_attrs [0] is MineAttribute)){
						Console.WriteLine ("Dit not get a MineAttribute");
						return 2;
					}

					MineAttribute ma = (MineAttribute) ret_attrs [0];
					if (ma.name != "Bar"){
						Console.WriteLine ("The return attribute is not Bar");
						return 2;
					}
				}
			}

			return 0;
		}
	}
}
