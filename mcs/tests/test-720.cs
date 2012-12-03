// Compiler options: -warn:4 -warnaserror

using System;

namespace N
{
	class Program
	{
		public static void Main ()
		{
			Parent pr = new Child();
			((Child)pr).OnExample();
		}
	}

	public abstract class Parent
	{
		public delegate void ExampleHandler();
		public abstract event ExampleHandler Example;
	}

	public class Child : Parent
	{
		public override event ExampleHandler Example;
		public void OnExample()
		{
			if (Example != null) Example();
		}
	}
}
