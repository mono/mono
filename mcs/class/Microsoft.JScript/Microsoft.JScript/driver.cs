//
// driver.cs: Guides the compilation process through the different phases.
//
// Author: 
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public class Driver {
		
		public static void Main (string [] args) {
	
			if (args.Length < 1) {
				Console.WriteLine ("Usage: [mono] mjs.exe filename.js");
				Environment.Exit (0);
			}

			string filename = args [0];
			Context ctx = new Context (filename);
			JSParser parser = new JSParser (ctx);

			ScriptBlock prog_tree = parser.Parse ();

			Console.WriteLine (prog_tree.ToString ());

			CodeGenerator.Run (args [0], prog_tree);
		}
	}
}
