//
// driver.cs: The compiler command line driver.
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
// Based on mcs by : Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2002 Rafael Teixeira
//

namespace Mono.Languages
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;
	using System.IO;
	using System.Globalization;
	using Mono.CSharp;

	enum Target {
		Library, Exe, Module, WinExe
	};
	
	/// <summary>
	///    The compiler driver.
	/// </summary>
	public class Driver
	{
		
		//
		// Assemblies references to be linked.   Initialized with
		// mscorlib.dll elsewhere.
		static ArrayList references;

		//
		// If any of these fail, we ignore the problem.  This is so
		// that we can list all the assemblies in Windows and not fail
		// if they are missing on Linux.
		//
		static ArrayList soft_references;

		// Lookup paths
		static ArrayList link_paths;

		// Whether we want to only run the tokenizer
		static bool tokenize = false;
		
		static int error_count = 0;

		static string first_source;

		static Target target = Target.Exe;
		static string target_ext = ".exe";

		static bool want_debugging_support = false;
		static ArrayList debug_arglist = new ArrayList ();

		static bool parse_only = false;
		static bool timestamps = false;

		//
		// Whether to load the initial config file (what CSC.RSP has by default)
		// 
		static bool load_default_config = true;

		static Hashtable response_file_list;
		static Hashtable source_files = new Hashtable ();

		//
		// An array of the defines from the command line
		//
		static ArrayList defines;

		//
		// A list of resource files
		//
		static ArrayList resources = new ArrayList();
		
		//
		// Last time we took the time
		//
		static DateTime last_time;
		static void ShowTime (string msg)
		{
			DateTime now = DateTime.Now;
			TimeSpan span = now - last_time;
			last_time = now;

			Console.WriteLine (
				"[{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}
	       
		
		static void Usage (bool is_error)
		{
			Console.WriteLine (	@"
MonoBASIC Compiler, Copyright (C)2002 Rafael Teixeira.
Usage: mbas [options] source-files
Options:
  --about         About the MonoBASIC compiler
  --checked       Set default context to checked
  --define SYM    Defines the symbol SYM
  --fatal         Makes errors fatal
  -g, --debug     Write symbolic debugging information to FILE-debug.s
  -h, --help      Prints this usage instructions
  -L PATH         Adds PATH to the assembly link path
  -m CLASS,
  --main CLASS    Specifies CLASS as main (starting) class
  --noconfig      Disables implicit references to assemblies
  --nostdlib      Does not load core libraries
  --nowarn XXX    Ignores warning number XXX
  -o FNAME,
  --output FNAME  Specifies output file
  --parse         Only parses the source file (for debugging the tokenizer)
  --probe X       Probes for the source to generate code X on line L
  -r ASSEMBLY     References an assembly
  --recurse SPEC  Recursively compiles the files in SPEC ([dir]/file)
  --resource FILE Adds FILE as a resource
  --stacktrace    Shows stack trace at error location
  --target KIND   Specifies the target (KIND is one of: exe, winexe, library, module)
  --tokenize      Only tokenizes source files
  --timestamp     Displays time stamps of various compiler events
  --unsafe        Allows unsafe code
  --werror        Treat warnings as errors
  -v              Verbose parsing (for debugging the parser)
  --wlevel LEVEL  Sets warning level (the highest is 4, the default)
  @file           Read response file for more options
");

		}


		static void About ()
		{
//			Options.ShowAbout();
		}
		
		static void error (string msg)
		{
			Console.WriteLine ("Error: " + msg);
		}

		static void notice (string msg)
		{
			Console.WriteLine (msg);
		}
		
//		private static Mono.GetOptions.OptionList Options;

		private static bool SetVerboseParsing(object nothing)
		{
			GenericParser.yacc_verbose_flag = true;
			return true;
		}

		public static int Main (string[] args)
		{
/*			Options = new Mono.GetOptions.OptionList("The compiler source code is released under the terms of the GNU GPL\n\n" +
				"For more information on Mono, visit the project Web site\n" +
				"   http://www.go-mono.com" , "mbas [options] source-files");
			Options.AddAbout(' ',"about", "About the MonoBASIC compiler");
			Options.AddBooleanSwitch ('v',"verbose", "Verbose parsing (for debugging the parser)", false, new Mono.GetOptions.OptionFound(SetVerboseParsing) );
*/			MainDriver(args);	
			return (error_count + Report.Errors) != 0 ? 1 : 0;
		}

		static public int LoadAssembly (string assembly, bool soft)
		{
			Assembly a;
			string total_log = "";

			try {
				char[] path_chars = { '/', '\\', '.' };

				if (assembly.IndexOfAny (path_chars) != -1)
					a = Assembly.LoadFrom (assembly);
				else
					a = Assembly.Load (assembly);
				TypeManager.AddAssembly (a);
				return 0;
			} catch (FileNotFoundException){
				foreach (string dir in link_paths){
					string full_path = dir + "/" + assembly + ".dll";

					try {
						a = Assembly.LoadFrom (full_path);
						TypeManager.AddAssembly (a);
						return 0;
					} catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				if (soft)
					return 0;
			} catch (BadImageFormatException f) {
				error ("// Bad file format while loading assembly");
				error ("Log: " + f.FusionLog);
				return 1;
			} catch (FileLoadException f){
				error ("File Load Exception: " + assembly);
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
		static public int LoadReferences ()
		{
			int errors = 0;

			foreach (string r in references)
				errors += LoadAssembly (r, false);

			foreach (string r in soft_references)
				errors += LoadAssembly (r, true);
			
			return errors;
		}

		static void SetupDefaultDefines ()
		{
			defines = new ArrayList ();
			defines.Add ("__MonoBASIC__");
		}

		static string [] LoadArgs (string file)
		{
			StreamReader f;
			ArrayList args = new ArrayList ();
			string line;
			try {
				f = new StreamReader (file);
			} catch {
				return null;
			}

			while ((line = f.ReadLine ()) != null){
				string [] line_args = line.Split (new char [] { ' ' });

				foreach (string arg in line_args)
					args.Add (arg);
			}

			string [] ret_value = new string [args.Count];
			args.CopyTo (ret_value, 0);

			return ret_value;
		}

		//
		// Returns the directory where the system assemblies are installed
		//
		static string GetSystemDir ()
		{
			Assembly [] assemblies = AppDomain.CurrentDomain.GetAssemblies ();

			foreach (Assembly a in assemblies){
				string codebase = a.CodeBase;
				if (codebase.EndsWith ("corlib.dll")){
					return codebase.Substring (0, codebase.LastIndexOf ("/"));
				}
			}

			Report.Error (-15, "Can not compute my system path");
			return "";
		}

		//
		// Given a path specification, splits the path from the file/pattern
		//
		static void SplitPathAndPattern (string spec, out string path, out string pattern)
		{
			int p = spec.LastIndexOf ("/");
			if (p != -1){
				//
				// Windows does not like /file.cs, switch that to:
				// "\", "file.cs"
				//
				if (p == 0){
					path = "\\";
					pattern = spec.Substring (1);
				} else {
					path = spec.Substring (0, p);
					pattern = spec.Substring (p + 1);
				}
				return;
			}

			p = spec.LastIndexOf ("\\");
			if (p != -1){
				path = spec.Substring (0, p);
				pattern = spec.Substring (p + 1);
				return;
			}

			path = ".";
			pattern = spec;
		}

		static int AddFile(string f)
		{
			if (first_source == null)
				first_source = f;

			if (source_files.Contains (f)){
				Report.Error (
					1516,
					"Source file `" + f + "' specified multiple times");
				Environment.Exit(1);
			} else
				source_files.Add(f, f);
					
			return 0;
		}

		static int ProcessSourceFile(string filename)
		{
			if (tokenize)
				GenericParser.Tokenize(filename);
			else
				return GenericParser.Parse(filename);

			return 0;
		}

		static int AddFiles (string spec, bool recurse)
		{
			string path, pattern;
			int errors = 0;

			SplitPathAndPattern (spec, out path, out pattern);
			if (pattern.IndexOf ("*") == -1){
				return AddFile (spec);
			}

			string [] files = null;
			try {
				files = Directory.GetFiles (path, pattern);
			} catch (System.IO.DirectoryNotFoundException) {
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return 1;
			} catch (System.IO.IOException){
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return 1;
			}
			foreach (string f in files)
				errors += AddFile (f);

			if (!recurse)
				return errors;
			
			string [] dirs = null;

			try {
				dirs = Directory.GetDirectories (path);
			} catch {
			}
			
			foreach (string d in dirs) {
					
				// Don't include path in this string, as each
				// directory entry already does
				errors += AddFiles (d + "/" + pattern, true);
			}
			

			return errors;
		}

		static void DefineDefaultConfig ()
		{
			//
			// For now the "default config" is harcoded into the compiler
			// we can move this outside later
			//
			string [] default_config = 
			{
				"System",
				"System.Data",
				"System.Xml",
				"Microsoft.VisualBasic", // just for now
#if false
				//
				// Is it worth pre-loading all this stuff?
				//
				"Accessibility",
				"System.Configuration.Install",
				"System.Design",
				"System.DirectoryServices",
				"System.Drawing.Design",
				"System.Drawing",
				"System.EnterpriseServices",
				"System.Management",
				"System.Messaging",
				"System.Runtime.Remoting",
				"System.Runtime.Serialization.Formatters.Soap",
				"System.Security",
				"System.ServiceProcess",
				"System.Web",
				"System.Web.RegularExpressions",
				"System.Web.Services",
				"System.Windows.Forms"
#endif
			};
			
			int p = 0;
			foreach (string def in default_config)
				soft_references.Insert (p++, def);
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
		static void MainDriver (string [] args)
		{
			int errors = 0, i;
			string output_file = null;
			bool parsing_options = true;
			
			references = new ArrayList ();
			soft_references = new ArrayList ();
			link_paths = new ArrayList ();
			SetupDefaultDefines ();
			
			//
			// Setup defaults
			//
			// This is not required because Assembly.Load knows about this
			// path.
			//
			link_paths.Add (GetSystemDir ());

//			Options.ProcessArgs(args);
//			return; 

			int argc = args.Length;
			for (i = 0; i < argc; i++){
				string arg = args [i];

				if (arg.StartsWith ("@")){
					string [] new_args, extra_args;
					string response_file = arg.Substring (1);

					if (response_file_list == null)
						response_file_list = new Hashtable ();
					
					if (response_file_list.Contains (response_file)){
						Report.Error (
							1515, "Response file `" + response_file +
							"' specified multiple times");
						Environment.Exit (1);
					}
					
					response_file_list.Add (response_file, response_file);
						    
					extra_args = LoadArgs (response_file);
					if (extra_args == null){
						Report.Error (2011, "Unable to open response file: " +
							      response_file);
						return;
					}

					new_args = new string [extra_args.Length + argc];
					args.CopyTo (new_args, 0);
					extra_args.CopyTo (new_args, argc);
					args = new_args;
					argc = new_args.Length;
					continue;
				}

				//
				// Prepare to recurse
				//
				
				if (parsing_options && (arg.StartsWith ("-"))){
					switch (arg){
					case "-v":
						GenericParser.yacc_verbose_flag = true;
						continue;

					case "--":
						parsing_options = false;
						continue;

					case "--parse":
						parse_only = true;
						continue;

					case "--main": case "-m":
						if ((i + 1) >= argc){
							Usage (true);
							return;
						}
						RootContext.MainClass = args [++i];
						continue;

					case "--unsafe":
						RootContext.Unsafe = true;
						continue;

					case "/?": case "/h": case "/help":
					case "--help":
						Usage (false);
						return;

					case "--define":
						if ((i + 1) >= argc){
							Usage (true);
							return;
						}
						defines.Add (args [++i]);
						continue;
						
					case "--probe": {
						int code = 0;

						try {
							code = Int32.Parse (
								args [++i], NumberStyles.AllowLeadingSign);
							Report.SetProbe (code);
						} catch {
							Report.Error (-14, "Invalid number specified");
						} 
						continue;
					}

					case "--tokenize": {
						tokenize = true;
						continue;
					}
					
					case "-o": 
					case "--output":
						if ((i + 1) >= argc){
							Usage (true);
							return;
						}
						output_file = args [++i];
						string bname = CodeGen.Basename (output_file);
						if (bname.IndexOf (".") == -1)
							output_file += ".exe";
						continue;

					case "--checked":
						RootContext.Checked = true;
						continue;

					case "--stacktrace":
						Report.Stacktrace = true;
						continue;

					case "--target":
						if ((i + 1) >= argc){
							Usage (true);
							return;
						}

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
						default:
							Usage (true);
							return;
						}
						continue;

					case "-r":
						if ((i + 1) >= argc){
							Usage (true);
							return;
						}
						
						references.Add(args [++i]);
						continue;
					
					case "--resource":
						if ((i + 1) >= argc)
						{
							Usage (true);
							Console.WriteLine("Missing argument to --resource"); 
							return;
						}
						
						resources.Add(args [++i]);
						continue;
					
					
					case "-L":
						if ((i + 1) >= argc){
							Usage (true);
							return;
						}
						link_paths.Add (args [++i]);
						continue;
						
					case "--nostdlib":
						RootContext.StdLib = false;
						continue;
						
					case "--fatal":
						Report.Fatal = true;
						continue;

					case "--werror":
						Report.WarningsAreErrors = true;
						continue;

					case "--nowarn":
						if ((i + 1) >= argc){
							Usage (true);
							return;
						}
						int warn;
						
						try {
							warn = Int32.Parse (args [++i]);
						} catch {
							Usage (true);
							return;
						}
						Report.SetIgnoreWarning (warn);
						continue;

					case "--wlevel":
						if ((i + 1) >= argc){
							Report.Error (
								1900,
								"--wlevel requires an value from 0 to 4");
							error_count++;
							return;
						}
						int level;
						
						try {
							level = Int32.Parse (args [++i]);
						} catch {
							Report.Error (
								1900,
								"--wlevel requires an value from 0 to 4");
							return;
						}
						if (level < 0 || level > 4){
							Report.Error (1900, "Warning level must be 0 to 4");
							return;
						} else
							RootContext.WarningLevel = level;
						continue;
						
					case "--about":
						About ();
						return;

					case "--recurse":
						if ((i + 1) >= argc){
							Console.WriteLine ("--recurse requires an argument");
							error_count++;
							return;
						}
						errors += AddFiles (args [++i], true);
						continue;
						
					case "--timestamp":
						timestamps = true;
						last_time = DateTime.Now;
						debug_arglist.Add("timestamp");
 						continue;

					case "--debug": case "-g":
						want_debugging_support = true;
						continue;

					case "--debug-args":
						if ((i + 1) >= argc){
							Console.WriteLine ("--debug-args requires an argument");
							error_count++;
							return;
						}
						char[] sep = { ',' };
						debug_arglist.AddRange (args [++i].Split (sep));
						continue;

					case "--noconfig":
						load_default_config = false;
						continue;

					default:
						Console.WriteLine ("Unknown option: " + arg);
						errors++;
						continue;
					}
				}

				// Rafael: Does not compile them yet!!!
				errors += AddFiles(arg, false); 
			}

			//Rafael: Compile all source files!!!
			foreach(string filename in source_files.Values)
				errors += ProcessSourceFile(filename);

			if (first_source == null)
			{
				Report.Error (2008, "No files to compile were specified");
				return;
			}

			if (tokenize)
				return;
			
			if (Report.Errors > 0)
				return;
			
			if (parse_only)
				return;
			
			//
			// Load Core Library for default compilation
			//
			if (RootContext.StdLib)
				references.Insert (0, "mscorlib");

			if (load_default_config)
				DefineDefaultConfig ();

			if (errors > 0){
				error ("Parsing failed");
				return;
			}

			//
			// Load assemblies required
			//
			if (timestamps)
				ShowTime ("Loading references");
			errors += LoadReferences ();
			if (timestamps)
				ShowTime ("   References loaded");
			
			if (errors > 0){
				error ("Could not load one or more assemblies");
				return;
			}

			error_count = errors;

			//
			// Quick hack
			//
			if (output_file == null){
				int pos = first_source.LastIndexOf (".");

				if (pos > 0)
					output_file = first_source.Substring (0, pos) + target_ext;
				else
					output_file = first_source + target_ext;
			}

			string[] debug_args = new string [debug_arglist.Count];
			debug_arglist.CopyTo(debug_args);
			CodeGen.Init (output_file, output_file, want_debugging_support, debug_args);

			TypeManager.AddModule (CodeGen.ModuleBuilder);

			//
			// Before emitting, we need to get the core
			// types emitted from the user defined types
			// or from the system ones.
			//
			if (timestamps)
				ShowTime ("Initializing Core Types");
			if (!RootContext.StdLib){
				RootContext.ResolveCore ();
				if (Report.Errors > 0)
					return;
			}
			
			TypeManager.InitCoreTypes ();
			if (timestamps)
				ShowTime ("   Core Types done");
		
			//
			// The second pass of the compiler
			//
			if (timestamps)
				ShowTime ("Resolving tree");
			RootContext.ResolveTree ();
			if (timestamps)
				ShowTime ("Populate tree");

			if (Report.Errors > 0){
				error ("Compilation failed");
				return;
			}

			if (!RootContext.StdLib)
				RootContext.BootCorlib_PopulateCoreTypes ();
			RootContext.PopulateTypes ();
			
			TypeManager.InitCodeHelpers ();
				
			if (Report.Errors > 0){
				error ("Compilation failed");
				return;
			}
			
			//
			// The code generator
			//
			if (timestamps)
				ShowTime ("Emitting code");
			RootContext.EmitCode ();
			if (timestamps)
				ShowTime ("   done");

			if (Report.Errors > 0){
				error ("Compilation failed");
				return;
			}

			if (timestamps)
				ShowTime ("Closing types");
			
			RootContext.CloseTypes ();

//			PEFileKinds k = PEFileKinds.ConsoleApplication;
//				
//			if (target == Target.Library || target == Target.Module)
//				k = PEFileKinds.Dll;
//			else if (target == Target.Exe)
//				k = PEFileKinds.ConsoleApplication;
//			else if (target == Target.WinExe)
//				k = PEFileKinds.WindowApplication;
//
//			if (target == Target.Exe || target == Target.WinExe){
//				MethodInfo ep = RootContext.EntryPoint;
//
//				if (ep == null){
//					Report.Error (5001, "Program " + output_file +
//							      " does not have an entry point defined");
//					return;
//				}
//				
//				CodeGen.AssemblyBuilder.SetEntryPoint (ep, k);
//			}

			//
			// Add the resources
			//
			if (resources != null){
				foreach (string file in resources)
					CodeGen.AssemblyBuilder.AddResourceFile (file, file);
			}
			
			CodeGen.Save (output_file);
			if (timestamps)
				ShowTime ("Saved output");

			if (want_debugging_support) {
				CodeGen.SaveSymbols ();
				if (timestamps)
					ShowTime ("Saved symbols");
			}

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
