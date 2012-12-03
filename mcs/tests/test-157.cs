using System;
using System.Reflection;

namespace Test {
	
	public class MyAttribute: Attribute {
		public string val;
		public MyAttribute (string stuff) {
			System.Console.WriteLine (stuff);
			val = stuff;
		}
	}

        public interface ITest {
                string TestProperty {
                        [My ("testifaceproperty")] get;
                }
        }
	
	[My("testclass")]
	public class Test {
		public static int Main () {
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

                        info = typeof (ITest).GetMethod ("get_TestProperty");
			attributes = info.GetCustomAttributes (false);
			for (int i = 0; i < attributes.Length; i ++) {
				System.Console.WriteLine(attributes[i]);
			}
			if (attributes.Length != 1)
				return 3;

                        attr = (MyAttribute) attributes [0];
			if (attr.val != "testifaceproperty")
				return 4;
                        
			return 0;
		}
	}
}
