using System;

namespace Test
{
	public static class Program
	{
		public static void Main ()
		{
		}
	}

	public abstract class Class1<T1>
	{
		protected event EventHandler doSomething;
	}

	public class Class2<T> : Class1<T>
	{
		public event EventHandler DoSomething
		{
			add { this.doSomething += value; }
			remove { this.doSomething -= value; }
		}
	}
}
