namespace Demo
{
	using System;

	public class Test
	{
		string inner;

		public Test ()
		{
			Console.WriteLine ("c# ctor invoked");
			inner = "c# instance method invoked";
		}

		public static void StaticMethod ()
		{
			Console.WriteLine ("c# static method invoked");
		}

		public void Method1 ()
		{
			Console.WriteLine (inner);
		}

		public void Method2 (string arg1)
		{
			Console.WriteLine (arg1);
		}

		public void Method3 (string arg1, int arg2)
		{
			Console.WriteLine (arg1 + arg2.ToString ());
		}

		public void GTypeGTypeGType ()
		{
			Console.WriteLine ("c# method with an unusual name invoked");
		}
	}
}
