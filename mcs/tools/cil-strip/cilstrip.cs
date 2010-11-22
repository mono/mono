//
// mono-cil-strip
//
// Author(s):
//   Jb Evain (jbevain@novell.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Reflection;

using Mono.Cecil;

namespace Mono.CilStripper {

	class Program {

		static void Main (string [] args)
		{
			Header ();

			if (args.Length == 0)
				Usage ();

			string file = args [0];
			string output = args.Length > 1 ? args [1] : file;

			try {
				AssemblyDefinition assembly = AssemblyFactory.GetAssembly (file);
				StripAssembly (assembly, output);

				if (file != output)
					Console.WriteLine ("Assembly {0} stripped out into {1}", file, output);
				else
					Console.WriteLine ("Assembly {0} stripped", file);
			} catch (TargetInvocationException tie) {
				Console.WriteLine ("Error: {0}", tie.InnerException);
			} catch (Exception e) {
				Console.WriteLine ("Error: {0}", e);
			}
		}

		static void StripAssembly (AssemblyDefinition assembly, string output)
		{
			AssemblyStripper.StripAssembly (assembly, output);
		}

		static void Header ()
		{
			Console.WriteLine ("Mono CIL Stripper");
			Console.WriteLine ();
		}

		static void Usage ()
		{
			Console.WriteLine ("Usage: mono-cil-strip file [output]");
			Environment.Exit (1);
		}
	}
}
