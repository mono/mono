using System.IO;
using System;

namespace CSC {
	public class Error {
		public static void report_error (string error) 
		{
			Console.Write ("ERROR: ");
			Console.WriteLine (error);
		}
	}
}
