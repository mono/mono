// Compiler options: -t:library
using System;

public class GlobalClass
{
	public int InstanceMethod ()
	{
		return 1;
	}
	
	public static int StaticMethod ()
	{
		return 1;
	}

	public static void JustForFirst ()
	{
	}
}

namespace Namespace1 
{
	public class MyClass1
	{
		public int InstanceMethod ()
		{
			return 1;
		}
		
		public static int StaticMethod ()
		{
			return 1;
		}
		
		public class MyNestedClass1
		{
			public int InstanceMethod ()
			{
				return 1;
			}

			public static int StaticMethod ()
			{
				return 1;
			}
		}

		public static void JustForFirst ()
		{
		}
	}

	namespace Namespace2
	{
		public class MyClass2
		{
			public class MyNestedClass2
			{
				public int InstanceMethod ()
				{
					return 1;
				}

				public static int StaticMethod ()
				{
					return 1;
				}
			}
			
			public int InstanceMethod ()
			{
				return 1;
			}
			
			public static int StaticMethod ()
			{
				return 1;
			}
			
			public static void JustForFirst ()
			{
			}
		}

	}
}

