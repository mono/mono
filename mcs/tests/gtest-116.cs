using System;

namespace Slow
{
	public interface ITest
	{
		void DoNothing<T>()
			where T : class;
	}

	public class Test : ITest
	{
		public void DoNothing<T>()
			where T : class
		{
			T x = null;
		}
	}

	class Program
	{
		public static void Main(string[] args)
		{
			const int iterations = 10000;

			Test test = new Test ();
            
			DateTime start = DateTime.Now;
			Console.Write ("Calling Test.DoNothing<Program>() on an object reference...  ");
			for (int i = 0; i < iterations; ++i)
			{
				test.DoNothing<Program> ();
			}
			DateTime end = DateTime.Now;
			TimeSpan duration = end - start;
			Console.WriteLine ("Took " + duration.TotalMilliseconds + " ms.");
	    
			ITest testInterface = test;

			start = DateTime.Now;
			Console.Write ("Calling Test.DoNothing<Program>() on an interface reference...  ");
			for (int i = 0; i < iterations; ++i)
			{
				testInterface.DoNothing<Program> ();
			}
			end = DateTime.Now;
			duration = end - start;
			Console.WriteLine ("Took " + duration.TotalMilliseconds + " ms.");
		}
	}
}
