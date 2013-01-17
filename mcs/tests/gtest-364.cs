using System;

namespace BugReport
{
	class Program
	{
		public static int Main()
		{
			A a = new A();
			a.Counter++;
			if (a.Counter != null)
				return 1;
			++a.Counter;
			if (a.Counter != null)
				return 2;
			
			a.Counter = 0;
			a.Counter++;
			if (a.Counter != 1)
				return 3;
			++a.Counter;
			if (a.Counter != 2)
				return 4;
			
			Console.WriteLine ("OK");
			return 0;
		}
	}

	class A {
		private int? _counter;
		public int? Counter {
			get { return _counter; }
			set { _counter = value; }
		}
	}
}

