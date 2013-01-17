using System;

namespace mono_bug
{
	class Program
	{
		public static void Main ()
		{

			// initialise so no null errors
			double [,] [] foo = new double [1, 1] [];
			foo [0, 0] = new double [2];

			double [,] [] bar;

			bar = (double [,] []) foo.Clone ();

			bar = (double [,] []) ReturnArray ();

			// compiles & works correctly
			bar = ReturnArray ();

			Console.WriteLine (bar [0, 0] [1].ToString ());
		}

		private static double [,] [] ReturnArray ()
		{
			// just creates a "useless", multi-dimensional jagged array
			double [,] [] zoo = new double [1, 1] [];
			zoo [0, 0] = new double [2];
			zoo [0, 0] [0] = 1;
			zoo [0, 0] [1] = 2;
			return zoo;
		}
	}
}
