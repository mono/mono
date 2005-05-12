//
// CompilerOptions.cs: The compiler command line options processor.
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
// Based on mcs by : Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2002, 2003, 2004 Rafael Teixeira
//

namespace Mono.MonoBASIC {

	using System;
	using System.Collections;
	using System.IO;
	using System.Text;
	using System.Reflection;
	using System.Reflection.Emit;

	using Mono.GetOptions;
	using Mono.GetOptions.Useful;

	public enum OptionCompare {
		Binary, Text
	};
	
	public delegate void ErrorReporter(int num, string msg);


	/// <summary>
	///    The compiler command line options processor.
	/// </summary>
	public class CompilerOptions : CommonCompilerOptions {
	
		ErrorReporter reportError;
	
		//
		// For now the "default config" is harcoded into the compiler
		// we can move this outside later
		//
		public string [] AssembliesToReferenceSoftly =  {
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


		public CompilerOptions(string [] args, ErrorReporter reportError) : base(args) { this.reportError = reportError; }
		
		private void LoadAssembly (AssemblyAdder adder, string assemblyName, ref int errors, bool soft)
		{
			Assembly a = null;
			string total_log = "";

			try  {
				char[] path_chars = { '/', '\\' };

				if (assemblyName.IndexOfAny (path_chars) != -1)
					a = Assembly.LoadFrom(assemblyName);
				else {
					string ass = assemblyName;
					if (ass.EndsWith (".dll"))
						ass = assemblyName.Substring (0, assemblyName.Length - 4);
					a = Assembly.Load (ass);
					adder(a);
					return;
				}
			}
			catch (FileNotFoundException) {
				if (PathsToSearchForLibraries != null) {
					foreach (string dir in PathsToSearchForLibraries) {
						string full_path = Path.Combine(dir, assemblyName + ".dll");

						try  {
							a = Assembly.LoadFrom (full_path);
							adder(a);
							return;
						} 
						catch (FileNotFoundException ff)  {
							total_log += ff.FusionLog;
							continue;
						}
					}
				}
				if (soft)
					return;
					
				reportError(6, "Can not find assembly '" + assemblyName + "'\nLog: " + total_log);
			}
			catch (BadImageFormatException f)  {
				reportError(6, "Bad file format while loading assembly\nLog: " + f.FusionLog);
			} catch (FileLoadException f){
				reportError(6, "File Load Exception: " + assemblyName + "\nLog: " + f.FusionLog);
			} catch (ArgumentNullException){
				reportError(6, "Argument Null exception");
			}
						
			errors++;
		}
		
		public delegate void AssemblyAdder (Assembly loadedAssembly);
		
		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		public bool LoadReferencedAssemblies (AssemblyAdder adder)
		{
			StartTime("Loading referenced assemblies");

			int errors = 0;
			int soft_errors = 0;
			
			// Load Core Library for default compilation
			if (!NoStandardLibraries)
				LoadAssembly(adder, "mscorlib", ref errors, false);

			foreach (string r in AssembliesToReference)
				LoadAssembly(adder, r, ref errors, false);

			if (!NoConfig)
				foreach (string r in AssembliesToReferenceSoftly)
					if (!(AssembliesToReference.Contains(r) || AssembliesToReference.Contains (r + ".dll")))
						LoadAssembly(adder, r, ref soft_errors, true);
			
			ShowTime("References loaded");
			return errors == 0;
		}
		
		public delegate void ModuleAdder (System.Reflection.Module module);
		
		private void LoadModule (MethodInfo adder_method, AssemblyBuilder assemblyBuilder, ModuleAdder adder, string module, ref int errors)
		{
			System.Reflection.Module m;
			string total_log = "";

			try {
				try {
					m = (System.Reflection.Module)adder_method.Invoke (assemblyBuilder, new object [] { module });
				}
				catch (TargetInvocationException ex) {
					throw ex.InnerException;
				}
				adder(m);
			} 
			catch (FileNotFoundException) {
				foreach (string dir in PathsToSearchForLibraries)	{
					string full_path = Path.Combine (dir, module);
					if (!module.EndsWith (".netmodule"))
						full_path += ".netmodule";

					try {
						try {
							m = (System.Reflection.Module) adder_method.Invoke (assemblyBuilder, new object [] { full_path });
						}
						catch (TargetInvocationException ex) {
							throw ex.InnerException;
						}
						adder(m);
						return;
					}
					catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				reportError(6, "Cannot find module `" + module + "'" );
				Console.WriteLine ("Log: \n" + total_log);
			}
			catch (BadImageFormatException f) {
				reportError(6, "Cannot load module (bad file format)" + f.FusionLog);
			}
			catch (FileLoadException f)	{
				reportError(6, "Cannot load module " + f.FusionLog);
			}
			catch (ArgumentNullException) {
				reportError(6, "Cannot load module (null argument)");
			}
			errors++;
		}

		public void UnsupportedFeatureOnthisRuntime(string feature)
		{
			reportError(0, string.Format("Cannot use {0} on this runtime: Try the Mono runtime instead.", feature));
			Environment.Exit (1);
		}

		public bool LoadAddedNetModules(AssemblyBuilder assemblyBuilder, ModuleAdder adder)
		{
			int errors = 0;
			
			if (NetModulesToAdd.Count > 0) {
				StartTime("Loading added netmodules");

				MethodInfo adder_method = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance|BindingFlags.NonPublic);
				if (adder_method == null)
					UnsupportedFeatureOnthisRuntime("/addmodule");

				foreach (string module in NetModulesToAdd)
					LoadModule (adder_method, assemblyBuilder, adder, module, ref errors);
					
				ShowTime("   Done");
			}
			
			return errors == 0;
		}
		
		public void AdjustCodegenWhenTargetIsNetModule(AssemblyBuilder assemblyBuilder)
		{
			if (TargetFileType == TargetType.Module) {
				StartTime("Adjusting AssemblyBuilder for NetModule target");
				PropertyInfo module_only = typeof (AssemblyBuilder).GetProperty ("IsModuleOnly", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if (module_only == null)
					UnsupportedFeatureOnthisRuntime("/target:module");

				MethodInfo set_method = module_only.GetSetMethod (true);
				set_method.Invoke (assemblyBuilder, BindingFlags.Default, null, new object[]{true}, null);
				ShowTime("   Done");
			}
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
				Report.Error (2001, "Source file '" + spec + "' could not be found");
				return false;
			} catch (System.IO.IOException){
				Report.Error (2001, "Source file '" + spec + "' could not be found");
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

		public void EmbedResources(AssemblyBuilder builder)
		{
			if (EmbeddedResources != null)
				foreach (string file in EmbeddedResources)
					builder.AddResourceFile (file, file); // TODO: deal with resource IDs
		}

		// Temporary options
		//------------------------------------------------------------------
		[Option("[IGNORED] Only parses the source file (for debugging the tokenizer)", "parse", SecondLevelHelp = true)]
		public bool OnlyParse = false;

		[Option("[IGNORED] Only tokenizes source files", "tokenize", SecondLevelHelp = true)]
		public bool Tokenize = false;

		[Option("Shows stack trace at Error location", "stacktrace", SecondLevelHelp = true)]
		public bool Stacktrace = false;
		
		[Option("Makes errors fatal", "fatal", SecondLevelHelp = true)]
		public bool MakeErrorsFatal = false;
		
		[Option("Displays time stamps of various compiler events", "timestamp", SecondLevelHelp = true)]
		public virtual bool PrintTimeStamps {
			set
			{
				printTimeStamps = true;
				last_time = DateTime.Now;
				DebugListOfArguments.Add("timestamp");
			}
		}

		// redefining some inherited options
		//------------------------------------------------------------------
		[Option("About the MonoBASIC compiler", "about")]
		public override WhatToDoNext DoAbout() { return base.DoAbout(); }

		[KillOption]
		public override WhatToDoNext DoUsage() { return WhatToDoNext.GoAhead; }

		// language options
		//------------------------------------------------------------------

		[Option("Require explicit declaration of variables", "optionexplicit", VBCStyleBoolean = true)]
		public bool OptionExplicit = false;

		[Option("Enforce strict language semantics", "optionstrict", VBCStyleBoolean = true)]
		public bool OptionStrict = false;
		
		[Option("Specifies binary-style string comparisons. This is the default", "optioncompare:binary")]
		public bool OptionCompareBinary = true; 

		[Option("Specifies text-style string comparisons.", "optioncompare:text")]
		public bool OptionCompareText { set { OptionCompareBinary = false; } }

		protected override void InitializeOtherDefaults() 
		{ 
			DefineSymbol = "__MonoBASIC__";
			ImportNamespaces = "Microsoft.VisualBasic";
		}
		
		private bool printTimeStamps = false;
		//
		// Last time we took the time
		//
		DateTime last_time;
		public void StartTime (string msg)
		{
			if (!printTimeStamps)
				return;
				
			last_time = DateTime.Now;

			Console.WriteLine("[*] {0}", msg);
		}

		public void ShowTime (string msg)
		{
			if (!printTimeStamps)
				return;
				
			DateTime now = DateTime.Now;
			TimeSpan span = now - last_time;
			last_time = now;

			Console.WriteLine (
				"[{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}
		
		public bool BeQuiet { get { return DontShowBanner || SuccintErrorDisplay; } } 
		
		public bool NothingToCompile {
			get {
				if (SourceFilesToCompile.Count == 0) {
					if (!BeQuiet) 
						DoHelp();
					return true;
				}
				if (!BeQuiet) {
					ShowBanner();
					// TODO: remove next lines when the compiler has matured enough
					Console.WriteLine ("--------");
					Console.WriteLine ("THIS IS AN ALPHA SOFTWARE.");
					Console.WriteLine ("--------");
				}		
				return false;
			}
		}
		
	}
}
