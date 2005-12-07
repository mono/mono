//
// CacheStress
//
// Authors:
//	Eyal Alayuf (Mainsoft)
//
// (c) 2005 Mainsoft, Inc. (http://www.mainsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Web.Caching;
using System.Threading;

public class CacheStress
{
	static int threads = 0;
	static int KeyStart = 0;
	static long SlidingWindow = 0;
	static bool UseAbsoluteTime = false;
	static int Modulo = 71;
	static Cache c;
	static SafeSum Sum = new SafeSum();

	static void Main(string[] args)
	{
		if (args.Length < 2) {
			Console.WriteLine("Usage: CacheStress <#threads> <#millis> [UseAbsoluteTime]");
			return;
		}
		c = new Cache();
		threads = System.Int32.Parse(args[0]);
		SlidingWindow = System.Int64.Parse(args[1]);
		UseAbsoluteTime = (args.Length > 2);
		for (int i = 0; i < threads; i++) 
		{
			Thread th = new Thread(new ThreadStart(RunCycle));
			th.Start();
		}
		int secs = 10;
		for (int j = secs; ;j += secs) 
		{
			Thread.Sleep(1000 * secs);
			Console.WriteLine("Executed {0} transactions in {1} seconds", Sum.Value, j);
		}
	}

	static void RunCycle()
	{
		int n = Interlocked.Increment(ref KeyStart);
		for (int i = 1; ; i++) {
			try 
			{
				string key = "stam" + n;
				object o2 = c.Get(key);
				if (o2 == null) 
				{
					if (UseAbsoluteTime)
						c.Insert(key, 1, null, DateTime.Now.AddTicks(SlidingWindow), Cache.NoSlidingExpiration);
					else
						c.Insert(key, 1, null, Cache.NoAbsoluteExpiration, new TimeSpan(SlidingWindow));
				}
				n = (n * 2 + i) % Modulo;
			}
			catch (Exception e) 
			{
				Console.WriteLine("Caught exception " + e.GetType().ToString() + ": " + e.Message + e.StackTrace);
			}
			if (i == 100) 
			{
				Sum.Add(i);
				i = 0;
			}
		}
	}

	class SafeSum
	{
		public SafeSum()
		{
			_value = 0;
		}

		public int Value { get { lock(this) { return _value; } } }
		public void Add(int i) { lock(this) { _value += i; } }

		private int _value;
	}
}
