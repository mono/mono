//
// driver.cs: The compiler command line driver.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

namespace CSC
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;
	using System.IO;
	using CIR;
	using Generator;
	using CSC;

	/// <summary>
	///    Summary description for Class1.
	/// </summary>
	public class Driver
	{
		//
		// Assemblies references to be linked.   Initialized with
		// mscorlib.dll here.
		ArrayList references;

		// Lookup paths
		ArrayList link_paths;

		// Our parser context.
		Tree context;
		
		bool yacc_verbose = false;

		int error_count = 0;

		public int parse (Tree context, string input_file)
		{
			CSharpParser parser;
			System.IO.Stream input;
			int errors;
			
			try {
				input = System.IO.File.OpenRead (input_file);
			} catch {
				return 1;
			}

			parser = new CSharpParser (context, input_file, input);
			parser.yacc_verbose = yacc_verbose;
			try {
				errors = parser.parse ();
			} catch (Exception ex) {
				Console.WriteLine (ex);
				Console.WriteLine ("Compilation aborted");
				return 1;
			}
			
			return errors;
		}
		
		public void Usage ()
		{
			Console.WriteLine (
				"compiler [-v] [-t tree] [-o output] [-L path] [-r reference] sources.cs\n" +
				"-v  Verbose parsing\n"+
				"-o  Specifies output file\n" +
				"-L  Specifies path for loading assemblies\n" +
				"-r  References an assembly\n");
			
		}

		public IGenerator lookup_output (string name)
		{
			if (name == "tree")
				return new Generator.TreeDump ();
			if (name == "il")
				return new MSIL.Generator ();
			
			return null;
		}

		public static void error (string msg)
		{
			Console.WriteLine ("Error: " + msg);
		}

		public static void notice (string msg)
		{
			Console.WriteLine (msg);
		}
		
		public static int Main(string[] args)
		{
			Driver driver = new Driver (args);

			return driver.error_count;
		}

		public int LoadAssembly (string assembly)
		{
			Assembly a;

			foreach (string dir in link_paths){
				string full_path = dir + "\\" + assembly;

				try {
					a = Assembly.Load (full_path);
				} catch (FileNotFoundException) {
					error ("// File not found: " + full_path);
					return 1;
				} catch (BadImageFormatException) {
					error ("// Bad file format: " + full_path);
					return 1;
				}

				context.AddAssembly (a);
			}
			return 0;
		}
		
		public int LoadReferences ()
		{
			int errors = 0;
			
			foreach (string r in references){
				errors += LoadAssembly (r);
			}

			return errors;
		}

		
		public Driver (string [] args)
		{
			Stream output_stream = Console.OpenStandardOutput ();
			IGenerator generator = null;
			int errors = 0, i;

			context = new Tree ();
			references = new ArrayList ();
			link_paths = new ArrayList ();

			//
			// Setup defaults
			//
			references.Add ("mscorlib.dll");
			link_paths.Add ("C://WINNT/Microsoft.Net/Framework/v1.0.2204");
			
			for (i = 0; i < args.Length; i++){
				string arg = args [i];
				
				if (arg.StartsWith ("-")){
					if (arg.StartsWith ("-v")){
						yacc_verbose = true;
						continue;
					}

					if (arg.StartsWith ("-t")){
						generator = lookup_output (args [++i]);
						continue;
					}

					if (arg.StartsWith ("-z")){
						generator.ParseOptions (args [++i]);
						continue;
					}
					
					if (arg.StartsWith ("-o")){
						try {
							output_stream = File.Create (args [++i]);
						} catch (Exception){
							error ("Could not write to `"+args [i]);
							error_count++;
							return;
						}
						continue;
					}

					if (arg.StartsWith ("-r")){
						references.Add (args [++i]);
						continue;
					}

					if (arg.StartsWith ("-L")){
						link_paths.Add (args [++i]);
						continue;
					}

					Usage ();
					error_count++;
					return;
				}
				
				if (!arg.EndsWith (".cs")){
					error ("Do not know how to compile " + arg);
					errors++;
					continue;
				}

				errors += parse (context, arg);
			}
			if (errors > 0)
				error ("// Parsing failed");
			else
				notice ("// Parsing successful");				

			//
			// Load assemblies required
			//
			errors += LoadReferences ();

			if (errors > 0)
				error ("// Could not load one or more assemblies");
			else
				notice ("// Assemblies loaded");


			errors += context.BuilderInit ("Module", "Module.exe");

			//
			// Name resolution on the tree.
			//
			errors += context.Resolve ();
						   
			//
			// Code generation from the tree
			//
			if (generator != null){
				StreamWriter output = new StreamWriter (output_stream);
				
				errors += generator.GenerateFromTree (context, output);

				if (errors > 0)
					error ("// Compilation failed");
				else
					notice ("// Compilation successful");

				output.Flush ();
				output.Close ();
			}

			error_count = errors;
		}

	}
}

