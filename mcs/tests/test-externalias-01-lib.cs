// Compiler options: -t:library

using System;

public class GlobalClass
{
	public int InstanceMethod ()
	{
		return 2;
	}
	
	public static int StaticMethod ()
	{
		return 2;
	}

	public static void JustForSecond ()
	{
	}
}

namespace Namespace1 
{
	public class MyClass1
	{
		public int InstanceMethod ()
		{
			return 2;
		}
		
		public static int StaticMethod ()
		{
			return 2;
		}
		
		public class MyNestedClass1
		{
			public int InstanceMethod ()
			{
				return 2;
			}

			public static int StaticMethod ()
			{
				return 2;
			}
		}

		public static void JustForSecond ()
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
					return 2;
				}

				public static int StaticMethod ()
				{
					return 2;
				}
			}
			
			public int InstanceMethod ()
			{
				return 2;
			}
			
			public static int StaticMethod ()
			{
				return 2;
			}

			public static void JustForFirst ()
			{
			}
		}

	}
}

