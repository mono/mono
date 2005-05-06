//
// driver.cs: The compiler command line driver.
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
// Based on mcs by : Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2002, 2003, 2004 Rafael Teixeira
//

namespace Mono.Languages {

	using System;
	using System.Collections;
//	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Globalization;
	using System.Reflection;
	using System.Reflection.Emit;

	using Mono.MonoBASIC;
	using Mono.GetOptions;
	using Mono.GetOptions.Useful;

	

	enum OptionCompare {
		Binary, Text
	};

		
	/// <summary>
	///    The compiler driver.
	/// </summary>
	public class Driver : CommonCompilerOptions {

		// Temporary options
		//------------------------------------------------------------------
		[Option("[Mono] Only parses the source file (for debugging the tokenizer)", "parse", SecondLevelHelp = true)]
		public bool parse_only = false;

		[Option("[IGNORED] Only tokenizes source files", SecondLevelHelp = true)]
		public bool tokenize = false;

		[Option("[Mono] Shows stack trace at Error location", SecondLevelHelp = true)]
		public bool stacktrace { set { Report.Stacktrace = value; } }

		[Option("[Mono] Displays time stamps of various compiler events", SecondLevelHelp = true)]
		public bool timestamp {
			set
			{
				timestamps = true;
				last_time = DateTime.Now;
				DebugListOfArguments.Add("timestamp");
			}
		}
		
		[Option("[Mono] Makes errors fatal", "fatal", SecondLevelHelp = true)]
		public bool Fatal { set { Report.Fatal = value; } }
		
		// redefining some inherited options
		//------------------------------------------------------------------
		[Option("About the MonoBASIC compiler", "about")]
		public override WhatToDoNext DoAbout()
		{
			return base.DoAbout();
		}

		[KillOption]
		public override WhatToDoNext DoUsage() { return WhatToDoNext.GoAhead; }

		// language options
		//------------------------------------------------------------------

//		[Option("[NOT IMPLEMENTED YET]Require explicit declaration of variables", VBCStyleBoolean = true)]
		public bool optionexplicit { set { Mono.MonoBASIC.Parser.InitialOptionExplicit = value; } }

//		[Option("[NOT IMPLEMENTED YET]Enforce strict language semantics", VBCStyleBoolean = true)]
		public bool optionstrict { set { Mono.MonoBASIC.Parser.InitialOptionStrict = value; } }
		
//		[Option("[NOT IMPLEMENTED YET]Specifies binary-style string comparisons. This is the default", "optioncompare:binary")]
		public bool optioncomparebinary { set { Mono.MonoBASIC.Parser.InitialOptionCompareBinary = true; } }

//		[Option("[NOT IMPLEMENTED YET]Specifies text-style string comparisons.", "optioncompare:text")]
		public bool optioncomparetext { set { Mono.MonoBASIC.Parser.InitialOptionCompareBinary = false; } }

		ArrayList soft_AssembliesToReference = new ArrayList();
		bool timestamps = false;

		//
		// Last time we took the time
		//
		DateTime last_time;
		void ShowTime (string msg)
		{
			DateTime now = DateTime.Now;
			TimeSpan span = now - last_time;
			last_time = now;

			Console.WriteLine (
				"[{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}
	       		
		public int LoadAssembly (string assembly, bool soft)
		{
			Assembly a;
			string total_log = "";

			try  {
				char[] path_chars = { '/', '\\' };

				if (assembly.IndexOfAny (path_chars) != -1)
					a = Assembly.LoadFrom(assembly);
				else {
					string ass = assembly;
					if (ass.EndsWith (".dll"))
						ass = assembly.Substring (0, assembly.Length - 4);
					a = Assembly.Load (ass);
				}
				TypeManager.AddAssembly (a);
				return 0;
			}
			catch (FileNotFoundException) {
				if (PathsToSearchForLibraries != null) {
					foreach (string dir in PathsToSearchForLibraries) {
						string full_path = dir + "/" + assembly + ".dll";

						try  {
							a = Assembly.LoadFrom (full_path);
							TypeManager.AddAssembly (a);
							return 0;
						} 
						catch (FileNotFoundException ff)  {
							total_log += ff.FusionLog;
							continue;
						}
					}
				}
				if (soft)
					return 0;
			}
			catch (BadImageFormatException f)  {
				Error ("// Bad file format while loading assembly");
				Error ("Log: " + f.FusionLog);
				return 1;
			} catch (FileLoadException f){
				Error ("File Load Exception: " + assembly);
				Error ("Log: " + f.FusionLog);
				return 1;
			} catch (ArgumentNullException){
				Error ("// Argument Null exception ");
				return 1;
			}
			
			Report.Error (6, "Can not find assembly `" + assembly + "'" );
			Console.WriteLine ("Log: \n" + total_log);

			return 0;
		}

		public void LoadModule (MethodInfo adder_method, string module)
		{
			System.Reflection.Module m;
			string total_log = "";

			try {
				try {
					m = (System.Reflection.Module)adder_method.Invoke (CodeGen.AssemblyBuilder, new object [] { module });
				}
				catch (TargetInvocationException ex) {
					throw ex.InnerException;
				}
				TypeManager.AddModule (m);

			} 
			catch (FileNotFoundException) {
				foreach (string dir in PathsToSearchForLibraries)	{
					string full_path = Path.Combine (dir, module);
					if (!module.EndsWith (".netmodule"))
						full_path += ".netmodule";

					try {
						try {
							m = (System.Reflection.Module) adder_method.Invoke (CodeGen.AssemblyBuilder, new object [] { full_path });
						}
						catch (TargetInvocationException ex) {
							throw ex.InnerException;
						}
						TypeManager.AddModule (m);
						return;
					}
					catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				Report.Error (6, "Cannot find module `" + module + "'" );
				Console.WriteLine ("Log: \n" + total_log);
			}
			catch (BadImageFormatException f) {
				Report.Error(6, "Cannot load module (bad file format)" + f.FusionLog);
			}
			catch (FileLoadException f)	{
				Report.Error(6, "Cannot load module " + f.FusionLog);
			}
			catch (ArgumentNullException) {
				Report.Error(6, "Cannot load module (null argument)");
			}
		}

		void Error(string message)
		{
			Console.WriteLine(message);
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		public int LoadReferences ()
		{
			int errors = 0;

			foreach (string r in AssembliesToReference)
				errors += LoadAssembly (r, false);

			foreach (string r in soft_AssembliesToReference)
				errors += LoadAssembly (r, true);
			
			return errors;
		}

		void SetupDefaultDefines ()
		{
			Defines.Add ("__MonoBASIC__", "true");
		}
		
		void SetupDefaultImports()
		{
			Mono.MonoBASIC.Parser.ImportsList = new ArrayList();
			Mono.MonoBASIC.Parser.ImportsList.Add("Microsoft.VisualBasic");
		}

		//
		// Given a path specification, splits the path from the file/pattern
		//
		void SplitPathAndPattern (string spec, out string path, out string pattern)
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

		bool AddFiles (string spec, bool recurse)
		{
			string path, pattern;

			SplitPathAndPattern(spec, out path, out pattern);
			if (pattern.IndexOf("*") == -1) {
				DefaultArgumentProcessor(spec);
				return true;
			}

			string [] files = null;
			try {
				files = Directory.GetFiles(path, pattern);
			} catch (System.IO.DirectoryNotFoundException) {
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return false;
			} catch (System.IO.IOException){
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return false;
			}
			foreach (string f in files)
				DefaultArgumentProcessor (f);

			if (!recurse)
				return true;
			
			string [] dirs = null;

			try {
				dirs = Directory.GetDirectories(path);
			} catch {
			}
			
			foreach (string d in dirs) {
					
				// Don't include path in this string, as each
				// directory entry already does
				AddFiles (d + "/" + pattern, true);
			}

			return true;
		}

		void DefineDefaultConfig ()
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
				"Microsoft.VisualBasic" , 
#if EXTRA_DEFAULT_REFS
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
				"System.Web.Services" ,
				"System.Windows.Forms"
#endif
			};
			
			foreach (string def in default_config)
				if (!(AssembliesToReference.Contains(def) || AssembliesToReference.Contains (def + ".dll")))
					soft_AssembliesToReference.Add(def);
		}

		void InitializeDebuggingSupport()
		{
			string[] debug_args = new string [DebugListOfArguments.Count];
			DebugListOfArguments.CopyTo(debug_args);
			CodeGen.Init(OutputFileName, OutputFileName, WantDebuggingSupport, debug_args);
			TypeManager.AddModule(CodeGen.ModuleBuilder);
		}

		public bool ResolveAllTypes() // Phase 2
		{
			// Load Core Library for default compilation
			if (RootContext.StdLib)
				AssembliesToReference.Insert(0, "mscorlib");

			if (!NoConfig)
				DefineDefaultConfig();

			if (timestamps)
				ShowTime("Loading referenced assemblies");

			// Load assemblies required
			if (LoadReferences() > 0) {
				Error ("Could not load one or more assemblies");
				return false;
			}

			if (timestamps)
				ShowTime("References loaded");

			InitializeDebuggingSupport();

			// TargetFileType is Module 
			if (TargetFileType == TargetType.Module) {
				PropertyInfo module_only = typeof (AssemblyBuilder).GetProperty ("IsModuleOnly", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if (module_only == null) {
					Report.Error (0, new Location (-1, -1), "Cannot use /TargetFileType:module on this runtime: try the Mono runtime instead.");
					Environment.Exit (1);
				}

				MethodInfo set_method = module_only.GetSetMethod (true);
				set_method.Invoke (CodeGen.AssemblyBuilder, BindingFlags.Default, null, new object[]{true}, null);

				TypeManager.AddModule (CodeGen.ModuleBuilder);
			}

			if (NetModulesToAdd.Count > 0) {
				MethodInfo adder_method = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance|BindingFlags.NonPublic);
				if (adder_method == null) {
					Report.Error (0, new Location (-1, -1), "Cannot use /addmodule on this runtime: Try the Mono runtime instead.");
					Environment.Exit (1);
				}

				foreach (string module in NetModulesToAdd)
					LoadModule (adder_method, module);
			}


			//
			// Before emitting, we need to get the core
			// types emitted from the user defined types
			// or from the system ones.
			//
			if (timestamps)
				ShowTime ("Initializing Core Types");

			if (!RootContext.StdLib)
				RootContext.ResolveCore ();
			if (Report.Errors > 0)
				return false;
			
			TypeManager.InitCoreTypes();
			if (Report.Errors > 0)
				return false;

			if (timestamps)
				ShowTime ("   Core Types done");

			if (timestamps)
				ShowTime ("Resolving tree");

			// The second pass of the compiler
			RootContext.ResolveTree ();
			if (Report.Errors > 0)
				return false;
			
			if (timestamps)
				ShowTime ("Populate tree");

			if (!RootContext.StdLib)
				RootContext.BootCorlib_PopulateCoreTypes();
			if (Report.Errors > 0)
				return false;

			RootContext.PopulateTypes();
			if (Report.Errors > 0)
				return false;
			
			TypeManager.InitCodeHelpers();
			if (Report.Errors > 0)
				return false;

			return true;
		}
		
		bool IsSWFApp()
		{
			string mainclass = GetFQMainClass();
			
			if (mainclass != null) {
				foreach (string r in AssembliesToReference) {
					if (r.IndexOf ("System.Windows.Forms") >= 0) {
						Type t = TypeManager.LookupType(mainclass);
						if (t != null) 
							return t.IsSubclassOf (TypeManager.LookupType("System.Windows.Forms.Form"));
						break;	
					}	
				}
			}
			return false;
		}
		
		string GetFQMainClass()
		{	
			if (RootContext.RootNamespace != "")
				return RootContext.RootNamespace + "." + RootContext.MainClass;
			else
				return RootContext.MainClass;			
		}
		
		void FixEntryPoint()
		{
			if (TargetFileType == TargetType.Exe || TargetFileType == TargetType.WinExe) {
				MethodInfo ep = RootContext.EntryPoint;
			
				if (ep == null) {
					// If we don't have a valid entry point yet
					// AND if System.Windows.Forms is included
					// among the dependencies, we have to build
					// a new entry point on-the-fly. Otherwise we
					// won't be able to compile SWF code out of the box.

					if (IsSWFApp())  {
						Type t = TypeManager.LookupType(GetFQMainClass());
						if (t != null) 
						{							
							TypeBuilder tb = t as TypeBuilder;
							MethodBuilder mb = tb.DefineMethod ("Main", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, 
								typeof(void), new Type[0]);

							Type SWFA = TypeManager.LookupType("System.Windows.Forms.Application");
							Type SWFF = TypeManager.LookupType("System.Windows.Forms.Form");
							Type[] args = new Type[1];
							args[0] = SWFF;
							MethodInfo mi = SWFA.GetMethod("Run", args);
							ILGenerator ig = mb.GetILGenerator();
							ConstructorInfo ci = TypeManager.GetConstructor (TypeManager.LookupType(t.FullName), new Type[0]);
							
							ig.Emit (OpCodes.Newobj, ci);
							ig.Emit (OpCodes.Call, mi);
							ig.Emit (OpCodes.Ret);

							RootContext.EntryPoint = mb as MethodInfo;
						}
					}
				}
			}
		}

		bool GenerateAssembly()
		{
			//
			// The code generator
			//
			if (timestamps)
				ShowTime ("Emitting code");
			
			

			RootContext.EmitCode();
			FixEntryPoint();
			if (Report.Errors > 0)
				return false;

			if (timestamps)
				ShowTime ("   done");


			if (timestamps)
				ShowTime ("Closing types");

			RootContext.CloseTypes ();
			if (Report.Errors > 0)
				return false;

			if (timestamps)
				ShowTime ("   done");

			PEFileKinds k = PEFileKinds.ConsoleApplication;
							
			if (TargetFileType == TargetType.Library || TargetFileType == TargetType.Module)
				k = PEFileKinds.Dll;
			else if (TargetFileType == TargetType.Exe)
				k = PEFileKinds.ConsoleApplication;
			else if (TargetFileType == TargetType.WinExe)
				k = PEFileKinds.WindowApplication;
			
			if (TargetFileType == TargetType.Exe || TargetFileType == TargetType.WinExe) {
				MethodInfo ep = RootContext.EntryPoint;
			
				if (ep == null) {
					Report.Error (30737, "Program " + OutputFileName +
						" does not have an entry point defined");
					return false;
				}
							
				CodeGen.AssemblyBuilder.SetEntryPoint (ep, k);
			}

			// Add the resources
			if (EmbeddedResources != null)
				foreach (string file in EmbeddedResources)
						CodeGen.AssemblyBuilder.AddResourceFile (file, file);
			
			CodeGen.Save(OutputFileName);

			if (timestamps)
				ShowTime ("Saved output");

			
			if (WantDebuggingSupport)  {
				CodeGen.SaveSymbols ();
				if (timestamps)
					ShowTime ("Saved symbols");
			}

			return true;
		}

		public void CompileAll()
		{
			try {
				InitializeRootContextFromOptions();
				
				if (!ParseAll()) // Phase 1
					return;

				if (!ResolveAllTypes()) // Phase 2
					return;

				GenerateAssembly(); // Phase 3 
				
			} catch (Exception ex) {
				Error("Exception: " + ex.ToString());
			}
		}
		
		private void InitializeRootContextFromOptions()
		{
			Report.WarningsAreErrors = WarningsAreErrors;
			// TODO: change Report to receive the whole array
			for(int i = 0; i < WarningsToIgnore.Length; i++)
				Report.SetIgnoreWarning(WarningsToIgnore[i]);

			RootContext.WarningLevel = WarningLevel;
			RootContext.Checked = CheckedContext;
			RootContext.MainClass = MainClassName;
			RootContext.StdLib = !NoStandardLibraries;
			RootContext.Unsafe = AllowUnsafeCode;
			RootContext.RootNamespace = RootNamespace;
			
			// TODO: semantics are different and should be adjusted
			GenericParser.yacc_verbose_flag = Verbose ? 1 : 0;
			
			foreach(string importedNamespace in Imports)
				Mono.MonoBASIC.Parser.ImportsList.Add(importedNamespace);
		}
		
		private bool quiet { get { return DontShowBanner || SuccintErrorDisplay; } } 
		
		private void Banner()
		{
			if (!quiet) {
				ShowBanner();
				// TODO: remove next lines when the compiler has matured enough
				Console.WriteLine ("--------");
				Console.WriteLine ("THIS IS AN ALPHA SOFTWARE.");
				Console.WriteLine ("--------");
			}		
		}
		
		protected void SetupDefaults()
		{
			SetupDefaultDefines();	
			SetupDefaultImports();
			Report.Stacktrace = false;
			RootContext.Checked = true;
		}		

		bool ParseAll() // Phase 1
		{
			foreach(FileToCompile file in SourceFilesToCompile)
				GenericParser.Parse(file.Filename, file.Encoding);

			return (Report.Errors == 0);
		}

		/// <summary>
		///    Parses the arguments, and calls the compilation process.
		/// </summary>
		int MainDriver(string [] args)
		{
			SetupDefaults();
			ProcessArgs(args);
			
			if (SourceFilesToCompile.Count == 0) {
				if (!quiet) 
					DoHelp();
				return 2;
			}

			Banner();			
			CompileAll();
			return Report.ProcessResults(quiet);
		}

		public static int Main (string[] args)
		{
			Driver Exec = new Driver();
			
			return Exec.MainDriver(args);
		}


	}
}
