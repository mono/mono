using System;

namespace MethodNames
{
	public class ClassA
	{
		public static int op_Addition()
		{
			return 0;
		}

		public static int operator +(ClassA x,int y)
		{
			return 10+y;
		}

		public ClassA ()
		{
		}
	}

	public class ClassB
	{
		public static int op_Addition;
		
		public ClassB ()
		{
		}
	}
}

