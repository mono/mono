//
// driver.cs: The compiler command line driver.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

namespace Mono.CSharp
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;
	using System.IO;
	using Mono.Languages;

	/// <summary>
	///    The compiler driver.
	/// </summary>
	public class Driver
	{
		enum Target {
			Library, Exe, Module, WinExe
		};
		
		//
		// Assemblies references to be linked.   Initialized with
		// mscorlib.dll here.
		ArrayList references;

		// Lookup paths
		ArrayList link_paths;

		RootContext context; 

		bool yacc_verbose = false;

		int error_count = 0;

		string first_source;

		Target target = Target.Exe;
		string target_ext = ".exe";

		bool parse_only = false;
		
		public int parse (string input_file)
		{
			CSharpParser parser;
			System.IO.Stream input;
			int errors;

			try {
				input = System.IO.File.OpenRead (input_file);
			} catch {
				Report.Error (2001, "Source file '" + input_file + "' could not be opened");
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
				"compiler [options] source-files\n\n" +
				"-v           Verbose parsing\n"+
				"-o           Specifies output file\n" +
				"-L           Specifies path for loading assemblies\n" +
				"--checked    Set default context to checked\n" +
				"--fatal      Makes errors fatal\n" +
				"--nostdlib   Does not load core libraries\n" +
				"--optimize   Optimizes\n" +
				"--parse      Only parses the source file\n" +
				"--probe X L  Probes for the source to generate code X on line L\n" +
				"--target     Specifies the target (exe, winexe, library, module)\n" +
				"-r           References an assembly\n");
			
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
			string total_log = "";

			try {
				a = Assembly.Load (assembly);
				context.TypeManager.AddAssembly (a);
				return 0;
			} catch (FileNotFoundException){
				foreach (string dir in link_paths){
					string full_path = dir + "/" + assembly + ".dll";

					try {
						a = Assembly.LoadFrom (full_path);
						context.TypeManager.AddAssembly (a);
						return 0;
					} catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
			} catch (BadImageFormatException f) {
				error ("// Bad file format while loading assembly");
				error ("Log: " + f.FusionLog);
				return 1;
			} catch (FileLoadException f){
				error ("// File Load Exception: ");
				error ("Log: " + f.FusionLog);
				return 1;
			} catch (ArgumentNullException){
				error ("// Argument Null exception ");
				return 1;
			}
			
			Report.Error (6, "Can not find assembly `" + assembly + "'" );
			Console.WriteLine ("Log: \n" + total_log);

			return 0;
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		public int LoadReferences ()
		{
			int errors = 0;
			
			foreach (string r in references){
				errors += LoadAssembly (r);
			}

			return errors;
		}

		/// <summary>
		///    Parses the arguments, and drives the compilation
		///    process.
		/// </summary>
		///
		/// <remarks>
		///    TODO: Mostly structured to debug the compiler
		///    now, needs to be turned into a real driver soon.
		/// </remarks>
		public Driver (string [] args)
		{
			ITreeDump generator = null;
			int errors = 0, i;
			string output_file = null;

			context = new RootContext ();
			references = new ArrayList ();
			link_paths = new ArrayList ();

			//
			// Setup defaults
			//
			// This is not required because Assembly.Load knows about this
			// path.
			//
			link_paths.Add ("file:///C:/WINNT/Microsoft.NET/Framework/v1.0.2914");
			
			for (i = 0; i < args.Length; i++){
				string arg = args [i];
				
				try {
					if (arg.StartsWith ("-")){
						if (arg == "-v"){
							yacc_verbose = true;
							continue;
						}
						
						if (arg == "--parse"){
							parse_only = true;
							continue;
						}

						if (arg == "--optimize"){
							RootContext.Optimize = true;
							continue;
						}
						
						if (arg == "--probe"){
							int code, line;

							code = Int32.Parse (args [++i], 0);
							line = Int32.Parse (args [++i], 0);
							Report.SetProbe (code, line);
							continue;
						}
						
						if (arg == "-z"){
							generator.ParseOptions (args [++i]);
							continue;
						}
						
						if (arg == "-o" || arg == "--output"){
							try {
								output_file = args [++i];
							} catch (Exception){
								error ("Could not write to `"+args [i]);
								error_count++;
								return;
							}
							continue;
						}

						if (arg == "--checked"){
							context.Checked = true;
							continue;
						}
						
						if (arg == "--target"){
							string type = args [++i];
							
							switch (type){
							case "library":
								target = Target.Library;
								target_ext = ".dll";
								break;
								
							case "exe":
								target = Target.Exe;
								break;
								
							case "winexe":
								target = Target.WinExe;
								break;
								
							case "module":
								target = Target.Module;
								target_ext = ".dll";
								break;
							}
							continue;
						}
						
						if (arg == "-r"){
							references.Add (args [++i]);
							continue;
						}
						
						if (arg == "-L"){
							link_paths.Add (args [++i]);
							continue;
						}
						
						if (arg == "--nostdlib"){
							context.StdLib = false;
							continue;
						}
						
						if (arg == "--fatal"){
							Report.Fatal = true;
							continue;
						}
						Usage ();
						error_count++;
						return;
					}
				} catch (System.IndexOutOfRangeException){
					Usage ();
					error_count++;
					return;
				} catch (System.FormatException) {
					Usage ();
					error_count++;
					return;
				}

				if (first_source == null)
					first_source = arg;

				if (!arg.EndsWith (".cs")){
					error ("Do not know how to compile " + arg);
					errors++;
					continue;
				}
				
				errors += parse (arg);
			}

			if (first_source == null){
				Report.Error (2008, "No files to compile were specified");
				return;
			}

			if (Report.Errors > 0)
				return;
			
			if (parse_only)
				return;
			
			//
			// Load Core Library for default compilation
			//
			if (context.StdLib){
				references.Insert (0, "mscorlib");
				references.Insert (1, "System");
			}

			if (errors > 0){
				error ("Parsing failed");
				return;
			}

			//
			// Load assemblies required
			//
			errors += LoadReferences ();

			if (errors > 0){
				error ("Could not load one or more assemblies");
				return;
			}


			//
			// Dumping the parsed tree.
			//
			// This code generation interface is only here
			// for debugging the parser. 
			//
			if (generator != null){
				if (output_file == null){
					error ("Error: no output file specified");
					return;
				}

				Stream output_stream = File.Create (output_file);
				StreamWriter output = new StreamWriter (output_stream);
				
				errors += generator.Dump (context.Tree, output);

				if (errors > 0){
					error ("Compilation failed");
					return;
				} else
					notice ("Compilation successful");

				output.Flush ();
				output.Close ();
			} 

			
			error_count = errors;

			//
			// Quick hack
			//
			if (output_file == null){
				int pos = first_source.LastIndexOf (".");

				output_file = first_source.Substring (0, pos) + target_ext;
			}

			context.CodeGen = new CodeGen (output_file, output_file);

			//
			// Before emitting, we need to get the core
			// types emitted from the user defined types
			// or from the system ones.
			//
			context.TypeManager.InitCoreTypes ();

			context.TypeManager.AddModule (context.CodeGen.ModuleBuilder);
			
			//
			// The second pass of the compiler
			//
			context.ResolveTree ();
			context.PopulateTypes ();
			
			if (Report.Errors > 0){
				error ("Compilation failed");
				return;
			}
			
			//
			// The code generator
			//
			context.EmitCode ();
			
			if (Report.Errors > 0){
				error ("Compilation failed");
				return;
			}
			
			context.CloseTypes ();

			PEFileKinds k = PEFileKinds.ConsoleApplication;
				
			if (target == Target.Library || target == Target.Module)
				k = PEFileKinds.Dll;
			else if (target == Target.Exe)
				k = PEFileKinds.ConsoleApplication;
			else if (target == Target.WinExe)
				k = PEFileKinds.WindowApplication;

			if (target == Target.Exe || target == Target.WinExe){
				MethodInfo ep = context.EntryPoint;

				if (ep == null){
					Report.Error (5001, "Program " + output_file +
							      " does not have an entry point defined");
					return;
				}
				
				context.CodeGen.AssemblyBuilder.SetEntryPoint (ep, k);
			}
			
			context.CodeGen.Save (output_file);

			if (Report.Errors > 0){
				error ("Compilation failed");
				return;
			} else if (Report.ProbeCode != 0){
				error ("Failed to report code " + Report.ProbeCode);
				Environment.Exit (124);
			}
		}

	}
}
