/* https://bugzilla.xamarin.com/show_bug.cgi?id=60862 */
using System;
using System.Threading;

namespace StackOverflowTest
{
	class Program
	{
		static bool fault = false;
		static Exception ex = null;

		public static int Main(string[] args)
		{
			Thread t = new Thread (Run);
			t.Start ();
			t.Join ();
			if (fault) {
				if (ex == null) {
					Console.WriteLine ("fault occurred, but no exception object available");
					return 1;
				} else {
					bool is_stackoverflow = ex is StackOverflowException;
					Console.WriteLine ("fault occurred: ex = " + is_stackoverflow);
					return is_stackoverflow ? 0 : 3;
				}
			}
			Console.WriteLine("no fault");
			return 2;
		}

	  static void Run()
	  {
		  try {
			  Execute ();
		  } catch(Exception e) {
			  ex = e;
			  fault = true;
		  }
	  }

	  static void Execute ()
	  {
		  WaitOne ();
	  }

	  static bool WaitOne (bool killProcessOnInterrupt = false, bool throwOnInterrupt = false)
	  {
		  try {
			  return WaitOne();
		  } catch(ThreadInterruptedException e) { }
		  return false;
	  }
  }
}
