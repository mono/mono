using System;
using System.Collections.Generic;

namespace test
{
	class Test<T>
	{
		public IEnumerable<T> Lookup(T item)
		{
			byte i = 3;
			byte j = 3;
			yield return item;
		}
	}

	class Program
	{
		public static void Main (string[] args)
		{
			Test<string> test = new Test<string>();
			foreach(string s in test.Lookup("hi") )
			{
				Console.WriteLine(s);
			}
		}
	}
}
