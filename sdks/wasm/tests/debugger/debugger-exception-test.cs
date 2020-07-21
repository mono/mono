using System;
using System.Threading.Tasks;
namespace DebuggerTests
{
	public class ExceptionTestsClass
	{
		public class TestCaughtException {
			public void run() {
				try {
					throw new Exception("not implemented");
				}
				catch {
					Console.WriteLine("caught exception");
				}
			}
		}

		public class TestUncaughtException {
			public void run() {
				throw new Exception("not implemented");
			}
		}

		public static void TestExceptions ()
		{
			TestCaughtException f = new TestCaughtException();
			f.run();

			TestUncaughtException g = new TestUncaughtException();
			g.run();
		}

	}

}
