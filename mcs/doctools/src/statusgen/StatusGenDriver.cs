using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using Mono.Doc.Utils;

namespace Mono.Doc.StatusGen
{
	class StatusGenDriver
	{
		private const string DEFAULT_OUTPUT_FILE = "out.xml";


		private StatusGenDriver()
		{
			// can't instantiate this class
		}


		public static void Main(string[] args)
		{
			string outFile   = DEFAULT_OUTPUT_FILE;
			string classFile = null;
			string statAssem = null;
			string diffAssem = null;
		
			if (args.Length < 2) {
				Usage();
				return;
			}

			for (int i = 0; i < args.Length; i++) {
				string arg = args[i];

				if (arg.StartsWith("-") && ((i + 1) < args.Length)) {
					if (arg == "--diff") {
						diffAssem = args[++i];
					} else if (arg == "--out") {
						outFile = args[++i];
					} else {
						Usage();
						return;
					}
				} else {
					if ((i + 1) >= args.Length) {
						Usage();
						return;
					} else {
						classFile = arg;
						statAssem = args[++i];
					}
				}
			}

			StatusGenerator generator =
				new StatusGenerator(statAssem, diffAssem, classFile, outFile);
			
			generator.Create();
		}


		private static void Usage()
		{
			Console.WriteLine(
				"Mono Assembly Status Generator\n" +
				"statusgen [options] CLASSXML ASSEMBLYFILE\n\n" +
				"    --out FILE  Specifies an output file\n" +
				"    --diff ASSEMBLYFILE  Specifies an assembly to diff against\n\n" +
				"CLASSXML is an XML file of the type used to generate Mono's maintainer list,\n" +
				"and is used to insert test suite and maintainer information.\n\n" +
				"ASSEMBLYFILE represents an assembly, either a file or, if in the format\n" +
				"'int:ASSEMBLYNAME', an assembly to be loaded from the current AppDomain.\n\n"
			);
		}
	}
}
