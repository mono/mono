// CS0206: A property, indexer or dynamic member access may not be passed as `ref' or `out' parameter
// Line: 22

using System;

namespace N
{
	public class Test
	{
		public double this[int i]
		{
			get { return 1; }
		}

		public static void WriteOutData(out double d)
		{
			d = 5.0;
		}

		public static void Main(string[] args)
		{
			Test test = new Test();
			WriteOutData(out test[1]);
		}
	}
}

