using System;
using System.Reflection;

namespace Test {
	[AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
	public class My1Attribute : Attribute
	{
		public My1Attribute (object o)
		{
			if (o != null)
				throw new ApplicationException ();
		}
	}

	public class My2Attribute : Attribute
	{
		public My2Attribute (string[] s)
		{
			if (s.Length != 0)
				throw new ApplicationException ();
		}
	}

	public class My3Attribute : Attribute
	{
		public My3Attribute (byte b)
		{
			if (b != 0xFF)
				throw new ApplicationException ();
		}
	}

	
	[My3(unchecked((byte)-1))]
	[My1((object)null)]
	[My1(null)]
	[My2(new string[0])]
	public class Test {
		public static int Main() {
			System.Reflection.MemberInfo info = typeof (Test);
			object[] attributes = info.GetCustomAttributes (false);
			
			if (attributes.Length != 4)
				return 1;

			for (int i = 0; i < attributes.Length; i ++) {
				Console.WriteLine (attributes [i]);
			}
			
			return 0;
		}
	}
}
