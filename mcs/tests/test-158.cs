using System;
using System.Reflection;

[AttributeUsage (AttributeTargets.All)]
public class My : Attribute {
	public object o;

	public My (object o) {
		this.o = o;
	}
	
	[My(TypeCode.Empty)]
	public class Test {
		public static int Main() {
			System.Reflection.MemberInfo info = typeof (Test);
			object[] attributes = info.GetCustomAttributes (false);
			for (int i = 0; i < attributes.Length; i ++) {
				System.Console.WriteLine(attributes[i]);
			}
			if (attributes.Length != 1)
				return 1;
			My attr = (My) attributes [0];
			if ((TypeCode) attr.o != TypeCode.Empty)
				return 2;
			return 0;
		}
	}
}
