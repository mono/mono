// cs-20.cs : Cannot find attribute type My (maybe you forgot to set the usage using the AttributeUsage attribute ?).
// Line : 18

using System;
using System.Reflection;

namespace Test {
	
       	//[AttributeUsage (AttributeTargets.All)]
	public class MyAttribute: Attribute {
		public string val;
		public MyAttribute (string stuff) {
			System.Console.WriteLine (stuff);
			val = stuff;
		}
	}
	
	[My("testclass")]

	public class Test {
		static public int Main() {
			System.Reflection.MemberInfo info = typeof (Test);
			object[] attributes = info.GetCustomAttributes (false);
			for (int i = 0; i < attributes.Length; i ++) {
				System.Console.WriteLine(attributes[i]);
			}
			if (attributes.Length != 1)
				return 1;
			MyAttribute attr = (MyAttribute) attributes [0];
			if (attr.val != "testclass")
				return 2;
			return 0;
		}
	}
}
