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

        [AttributeUsage (AttributeTargets.ReturnValue)]
	public class ReturnAttribute : Attribute {

		public string name;
		
		public ReturnAttribute (string s)
		{
			name = s;
		}
	}

	public interface TestInterface {
		void Hello ([Mine ("param")] int i);
	}	

	public class Foo {	
		
		int i;
		
		[Mine ("Foo")]
		[return: Return ("Bar")]	
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
					if (!(ret_attrs [0] is ReturnAttribute)){
						Console.WriteLine ("Dit not get a MineAttribute");
						return 2;
					}

					ReturnAttribute ma = (ReturnAttribute) ret_attrs [0];
					if (ma.name != "Bar"){
						Console.WriteLine ("The return attribute is not Bar");
						return 2;
					}
				}
			}

                        Type ifType = typeof (TestInterface);
                        
                        MethodInfo method = ifType.GetMethod ("Hello", 
                                                              BindingFlags.Public | BindingFlags.Instance);
                        
                        ParameterInfo[] parameters = method.GetParameters();
                        ParameterInfo param = parameters [0];
                        
                        object[] testAttrs = param.GetCustomAttributes (true);
                        
                        if (testAttrs.Length != 1)
                                return 1;
                        
			return 0;
		}
	}
}
