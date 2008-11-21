// Mono.Util.CorCompare.CorCompareDriver
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.IO;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Handles command line arguments, and generates appropriate report(s)
	/// 	based on those arguments
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	public class CorCompareDriver
	{
		public static void Main(string[] args) {
			// make sure we were called with the proper usage
			if (args.Length < 1) {
				Console.WriteLine("Usage: CorCompare [-t][-n][-x outfile][-ms assembly][-f friendly_name] assembly_to_compare");
				return;
			}

			bool fList = false;
			string strXML = null;
			string strMono = args [args.Length - 1];
			string strMS = null;
			string strFriendly = null;

			for (int i = 0; i < args.Length-1; i++) {
				if (args [i] == "-t") {
					fList = true;
				}
				if (args [i] == "-n") {
				}
				if (args [i] == "-x") {
					strXML = args [++i];
				}
				if (args [i] == "-ms") {
					strMS = args [++i];
				}
				if (args [i] == "-f") {
					strFriendly = args [++i];
				}
			}

			if (strMS == null)
				strMS = Path.GetFileNameWithoutExtension (strMono);

			if (strFriendly == null)
				strFriendly = strMS;

			if (strXML == null)
				strXML = strFriendly + ".xml";

			ToDoAssembly td = ToDoAssembly.Load (strMono, strFriendly, strMS);

			if (fList)
				Console.WriteLine(td.CreateClassListReport());

			if (strXML != null)
				td.CreateXMLReport(strXML);

		}
	}
}
