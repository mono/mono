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
	using Mono.GetOptions;

	enum Target 
	{
		Library, Exe, Module, WinExe
	};
	
	/// <summary>
	///    The compiler driver.
	/// </summary>
	public class Driver : Options
	{
		
		[Option("Verbose parsing (for debugging the parser)",'v')] 
		public bool verbose	{ set { GenericParser.yacc_verbose_flag = value; } }

		[Option("Specifies PARAM as main (starting) class", 'm')]
		public string main { set { RootContext.MainClass = value; } }

		[Option("About the MonoBASIC compiler", "about")]
		public override WhatToDoNext DoAbout()
		{
			return base.DoAbout();
		}

		[Option("Adds PARAM to the assembly link path", 'L')]
		public static string[] LinkPaths = null;

		[Option("Defines the symbol PARAM", "define")]
		public static string[] Defines = null;

		[Option("Only parses the source file (for debugging the tokenizer)", "parse")]
		public static bool parse_only = false;

		private static bool load_default_config = true;

		[Option("Disables implicit references to assemblies", "noconfig")]
		public bool NoConfig { set { load_default_config = !value; } }

		[Option("Allows unsafe code", "unsafe")]
		public bool AllowUnsafeCode { set { RootContext.Unsafe = value; } }

		private string output_file;

		[Option("Specifies output file", 'o', "output")]
		public WhatToDoNext SetOutputFile(string FileName)
		{
			output_file = FileName;
			string bname = CodeGen.Basename (output_file);
			if (bname.IndexOf (".") == -1)
				output_file += ".exe";
			return WhatToDoNext.GoAhead;
		}


		[Option("Only tokenizes source files", "tokenize")]
		public static bool tokenize = true;

		[Option("Set default context to checked", "checked")]
		public bool Checked { set { RootContext.Checked = value; } }

		[Option("Shows stack trace at error location", "Stacktrace")]
		public bool Stacktrace { set { Report.Stacktrace = value; } }

		private static ArrayList references;

		[Option("References an assembly", 'r')]
		public static string reference { set { references.Add(value); } }

		[Option("Adds PARAM as a resource", "resource")]
		public static string[] resources;

		[Option("Set default context to checked", "nostdlib")]
		public bool nostdlib { set { RootContext.StdLib = !value; } }

		[Option("Makes errors fatal", "fatal")]
		public bool Fatal { set { Report.Fatal = value; } }

		[Option("Treat warnings as errors", "werror")]
		public bool WarningsAreErrors { set { Report.WarningsAreErrors = value; } }

		[Option("Ignores warning number PARAM", "nowarn")]
		public WhatToDoNext SetIgnoreWarning(int warn)
		{
			Report.SetIgnoreWarning(warn);
			return WhatToDoNext.GoAhead;
		}

		[Option("Recursively compiles the files in PARAM ([dir]/file)", "recurse")]
		public WhatToDoNext recurse(string DirName)
		{
			AddFiles (DirName, true);
			return WhatToDoNext.GoAhead;
		}
	

		[Option("Write symbolic debugging information to FILE-debug.s", 'g', "debug")]
		public static bool want_debugging_support = false;

		[Option("Debugger arguments", "debug-args")]
		public WhatToDoNext SetDebugArgs(string args)
		{
			char[] sep = { ',' };
			debug_arglist.AddRange (args.Split (sep));
			return WhatToDoNext.GoAhead;
		}

		[Option("Specifies the target (PARAM is one of: exe, winexe, library, module)", "target")]
		public WhatToDoNext SetTarget(string type)
		{
			switch (type)
			{
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
			return WhatToDoNext.GoAhead;
		}

		[Option("Sets warning level (the highest is 4, the default)", "wlevel")]
		public int wlevel { set { RootContext.WarningLevel = value; } }

		[Option("Sets warning level (the highest is 4, the default)")]
		public bool timestamp
		{
			set
			{
				timestamps = true;
				last_time = DateTime.Now;
				debug_arglist.Add("timestamp");
			}
		}

	static void Usage (bool is_error)
		{
			Console.WriteLine (	@"
MonoBASIC Compiler, Copyright (C)2002 Rafael Teixeira.
  --timestamp     Displays time stamps of various compiler events
  @file           Read response file for more options
");
		}
		
		//
		// If any of these fail, we ignore the problem.  This is so
		// that we can list all the assemblies in Windows and not fail
		// if they are missing on Linux.
		//
		static ArrayList soft_references;

		static int error_count = 0;

		static string first_source;

		static Target target = Target.Exe;
		static string target_ext = ".exe";

		static ArrayList debug_arglist = new ArrayList ();

		static bool timestamps = false;

		static Hashtable source_files = new Hashtable ();

		//
		// An array of the defines from the command line
		//
		static ArrayList defines;

		
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
	       		
		static void error (string msg)
		{
			Console.WriteLine ("Error: " + msg);
		}

		static void notice (string msg)
		{
			Console.WriteLine (msg);
		}
		
		public static int Main (string[] args)
		{
			Driver Exec = new Driver();
			
			if (Exec.MainDriver(args) && Report.Errors == 0) 
			{
				Console.Write("Compilation succeeded");
				if (Report.Warnings > 0) 
				{
					Console.Write(" - {0} warning(s)", Report.Warnings);
				} 
				Console.WriteLine();
				return 0;
			} 
			else 
			{
				Console.WriteLine("Compilation failed: {0} error(s), {1} warnings",
					Report.Errors, Report.Warnings);
				return 1;
			}
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
				foreach (string dir in LinkPaths){
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


		static int ProcessSourceFile(string filename)
		{
			if (tokenize)
				GenericParser.Tokenize(filename);
			else
				return GenericParser.Parse(filename);

			return 0;
		}

		static bool AddFiles (string spec, bool recurse)
		{
			string path, pattern;

			SplitPathAndPattern (spec, out path, out pattern);
			if (pattern.IndexOf ("*") == -1){
				return AddFile (spec);
			}

			string [] files = null;
			try {
				files = Directory.GetFiles (path, pattern);
			} catch (System.IO.DirectoryNotFoundException) {
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return false;
			} catch (System.IO.IOException){
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return false;
			}
			foreach (string f in files)
				AddFile (f);

			if (!recurse)
				return true;
			
			string [] dirs = null;

			try {
				dirs = Directory.GetDirectories (path);
			} catch {
			}
			
			foreach (string d in dirs) {
					
				// Don't include path in this string, as each
				// directory entry already does
				AddFiles (d + "/" + pattern, true);
			}
			

			return true;
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

		private static bool AddFile(string fileName)
		{
			string f = fileName;
			if (first_source == null)
				first_source = f;

			if (source_files.Contains(f))
			{
				Report.Error (1516, "Source file `" + f + "' specified multiple times");
				return false;
			} 
			else
				source_files.Add(f, f);

			return true;
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
		bool MainDriver(string [] args)
		{
			int errors = 0;//, i;
			string output_file = null;
			
			soft_references = new ArrayList ();
			SetupDefaultDefines ();
			
			this.ProcessArgs(args);

			foreach(string arg in this.RemainingArguments)
				AddFile(arg); 

			foreach(string filename in source_files.Values)
				errors += ProcessSourceFile(filename);

			if (first_source == null)
			{
				Report.Error (2008, "No files to compile were specified");
				return false;
			}

			if (tokenize)
				return true;
			
			if (Report.Errors > 0)
				return false;
			
			if (parse_only)
				return true;
			
			//
			// Load Core Library for default compilation
			//
			if (RootContext.StdLib)
				references.Insert (0, "mscorlib");

			if (load_default_config)
				DefineDefaultConfig ();

			if (errors > 0)
			{
				error ("Parsing failed");
				return false;
			}

			//
			// Load assemblies required
			//
			if (timestamps)
				ShowTime ("Loading references");
			errors += LoadReferences ();
			if (timestamps)
				ShowTime ("   References loaded");
			
			if (errors > 0)
			{
				error ("Could not load one or more assemblies");
				return false;
			}

			error_count = errors;

			//
			// Quick hack
			//
			if (output_file == null)
			{
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
			if (!RootContext.StdLib)
			{
				RootContext.ResolveCore ();
				if (Report.Errors > 0)
					return false;
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

			if (Report.Errors > 0)
			{
				error ("Compilation failed");
				return false;
			}

			if (!RootContext.StdLib)
				RootContext.BootCorlib_PopulateCoreTypes ();
			RootContext.PopulateTypes ();
			
			TypeManager.InitCodeHelpers ();
				
			if (Report.Errors > 0)
			{
				error ("Compilation failed");
				return false;
			}
			
			//
			// The code generator
			//
			if (timestamps)
				ShowTime ("Emitting code");
			RootContext.EmitCode ();
			if (timestamps)
				ShowTime ("   done");

			if (Report.Errors > 0)
			{
				error ("Compilation failed");
				return false;
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
			if (resources != null)
			{
				foreach (string file in resources)
					CodeGen.AssemblyBuilder.AddResourceFile (file, file);
			}
			
			CodeGen.Save (output_file);
			if (timestamps)
				ShowTime ("Saved output");

			if (want_debugging_support) 
			{
				CodeGen.SaveSymbols ();
				if (timestamps)
					ShowTime ("Saved symbols");
			}

			if (Report.ExpectedError != 0)
			{
				Console.WriteLine("Failed to report expected error " + Report.ExpectedError);
				Environment.Exit (1);
				return false;
			}
			return (Report.Errors == 0);
		}

	}
}
