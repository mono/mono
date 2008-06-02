using System;

namespace MonoTests
{
	public abstract class MainClass
	{
		protected virtual string [] foo { get; set; }
		public abstract string [] bar { get; set; }

		public static void Main (string [] args)
		{
			Console.WriteLine ("Hello World!");
		}
	}
	public class ChildClass : MainClass
	{
		protected override string [] foo { get; set; }
		public override string [] bar { get; set; }
	}
}
