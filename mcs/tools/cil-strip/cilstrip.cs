//
// mono-cil-strip
//
// Author(s):
//   Jb Evain (jbevain@novell.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections.Generic;
using System.Reflection;

using Mono.Cecil;

namespace Mono.CilStripper {

	class Program {
		static bool quiet;
		static int Main (string [] arguments)
		{
			var args = new List<string> (arguments);
			if (args.Count > 0 && args [0] == "-q") {
				quiet = true;
				args.RemoveAt (0);
			}
			Header ();

			if (args.Count == 0)
				Usage ();

			string file = args [0];
			string output = args.Count > 1 ? args [1] : file;

			try {
				AssemblyDefinition assembly = AssemblyFactory.GetAssembly (file);
				StripAssembly (assembly, output);

				if (!quiet) {
					if (file != output)
						Console.WriteLine ("Assembly {0} stripped out into {1}", file, output);
					else
						Console.WriteLine ("Assembly {0} stripped", file);
				}
				return 0;
			} catch (TargetInvocationException tie) {
				Console.WriteLine ("Error: {0}", tie.InnerException);
			} catch (Exception e) {
				Console.WriteLine ("Error: {0}", e);
			}
			return 1;
		}

		static void StripAssembly (AssemblyDefinition assembly, string output)
		{
			AssemblyStripper.StripAssembly (assembly, output);
		}

		static void Header ()
		{
			if (quiet)
				return;
			Console.WriteLine ("Mono CIL Stripper");
			Console.WriteLine ();
		}

		static void Usage ()
		{
			Console.WriteLine ("Usage: mono-cil-strip [options] file [output]");
			Console.WriteLine ("    -q         Only output errors.");
			Environment.Exit (1);
		}
	}
}
