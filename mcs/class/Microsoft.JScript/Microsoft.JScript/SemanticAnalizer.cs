//
// SemanticAnalyser.cs: Initiate the type check and identification phases.
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

namespace Microsoft.JScript {

	public class SemanticAnalyser {

		//
		// We must include the default 'Global Object',
		// which contains the built-in objects: Math, String, ...
		// static void init_default_global_object ()
		//

		static IdentificationTable context;

		public static bool Run (ScriptBlock prog)
		{
			context = new IdentificationTable ();

			return prog.Resolve (context);
		}

		public static void Dump ()
		{
			Console.WriteLine (context.ToString ());
		}
	}
}
