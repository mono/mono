// Mono.Util.CorCompare.CorCompareDriver
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Handles command line arguments, and generates appropriate report(s) 
	/// 	based on those arguments
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class CorCompareDriver
	{
		public static void Main(string[] args) {
			// make sure we were called with the proper usage
			if (args.Length < 1) {
				Console.WriteLine("Usage: CorCompare [-t][-n][-x outfile] assembly_to_compare");
				return;
			}

			ToDoAssembly td = new ToDoAssembly(args[args.Length-1], "corlib");

			for (int i = 0; i < args.Length-1; i++) {
				if (args [i] == "-t") {
					Console.WriteLine(td.CreateClassListReport());
				}
				if (args [i] == "-n") {
				}
				if (args [i] == "-x") {
                    td.CreateXMLReport(args[++i]);
				}
			}
		}
	}
}
