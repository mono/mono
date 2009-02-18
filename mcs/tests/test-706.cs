using System;

namespace Test
{
	public abstract class CustomParentAttribute : Attribute
	{
		public abstract void DoSomething ();
	}

	[CustomChild]
	public class MyClass
	{
		private sealed class CustomChildAttribute : CustomParentAttribute
		{
			public override void DoSomething ()
			{
			}
		}
		
		
		public static void Main ()
		{
		}
	}
}
