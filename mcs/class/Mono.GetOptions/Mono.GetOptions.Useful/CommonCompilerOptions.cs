//
// CommonCompilerOptions.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2005 Rafael Teixeira
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Mono.GetOptions.Useful
{

	public enum TargetType {
		Library, Exe, Module, WinExe
	};
	
	public struct FileToCompile {
		public string Filename;
		public Encoding Encoding;
		
		public FileToCompile(string filename, Encoding encoding)
		{
			this.Filename = filename;
			this.Encoding = encoding;	
		}
	}

	public enum InternalCompilerErrorReportAction { 
		prompt, send, none 
	}
		
	public delegate void ModuleAdder (System.Reflection.Module module);
	public delegate void AssemblyAdder (Assembly loadedAssembly);
	
	public class CommonCompilerOptions : Options {
	
		public CommonCompilerOptions() : this(null, null) { }

		public CommonCompilerOptions(string[] args) : this(args, null) {}

		public CommonCompilerOptions(string[] args, ErrorReporter reportError) : base(args, OptionsParsingMode.Both, false, true, true, reportError) 
		{
			PathsToSearchForLibraries.Add (Directory.GetCurrentDirectory ());
		}
		
		[Option(-1, "References packages listed. {packagelist}=package,...", "pkg")]
		public WhatToDoNext ReferenceSomePackage(string packageName)
		{
			return ReferencePackage(packageName)?WhatToDoNext.GoAhead:WhatToDoNext.AbandonProgram;
		}

		private Encoding currentEncoding = null;
		
		[Option(-1, "Select codepage by {ID} (number, 'utf8' or 'reset') to process following source files", "codepage")]
		public string CurrentCodepage {
			set {
				switch (value.ToLower()) {
					case "reset": 
						currentEncoding = null; 
						break;
					case "utf8": case "utf-8":
						currentEncoding = Encoding.UTF8;
						break;
					default:
						try {
							currentEncoding = Encoding.GetEncoding(int.Parse(value));
						} catch (NotSupportedException) {
							ReportError (0, string.Format("Ignoring unsupported codepage number {0}.", value));
						} catch (Exception) {
							ReportError (0, string.Format("Ignoring unsupported codepage ID {0}.", value));
						}
						break;
				}					
			}
		}
		
		private ArrayList warningsToIgnore = new ArrayList();
		public int[] WarningsToIgnore { get { return (int[])warningsToIgnore.ToArray(typeof(int)); } }
		
		[Option(-1, "Ignores warning number {XXXX}", "ignorewarn", SecondLevelHelp = true)]
		public WhatToDoNext SetIgnoreWarning(int warningNumber)
		{
			warningsToIgnore.Add(warningNumber);
			return WhatToDoNext.GoAhead;
		}	
		
		[Option("Sets warning {level} (the highest is 4, the default)", "wlevel", SecondLevelHelp = true)]
		public int WarningLevel = 4; 

		// Output file options
		//------------------------------------------------------------------
		public TargetType TargetFileType = TargetType.Exe;

		string outputFileName = null;
		string firstSourceFile = null;
		string targetFileExtension = ".exe";

		[Option("Specifies the output {file} name", 'o', "out")]
		public string OutputFileName 
		{
			set { outputFileName = value; }
			get 
			{
				if (outputFileName == null) {
					int pos = firstSourceFile.LastIndexOf(".");

					if (pos > 0)
						outputFileName = firstSourceFile.Substring(0, pos);
					else
						outputFileName = firstSourceFile;
// TODO: what Codegen does here to get hid of this dependency
//					string bname = CodeGen.Basename(outputFileName);
//					if (bname.IndexOf(".") == -1)
						outputFileName +=  targetFileExtension;
				}
				return outputFileName;
			}
		}


		[Option("Specifies the target {type} for the output file (exe [default], winexe, library, module)", 't', "target")]
		public WhatToDoNext SetTarget(string type)
		{
			switch (type.ToLower()) {
				case "library":
					TargetFileType = TargetType.Library;
					targetFileExtension = ".dll";
					break;
							
				case "exe":
					TargetFileType = TargetType.Exe;
					targetFileExtension = ".exe";
					break;
							
				case "winexe":
					TargetFileType = TargetType.WinExe;
					targetFileExtension = ".exe";
					break;
							
				case "module":
					TargetFileType = TargetType.Module;
					targetFileExtension = ".netmodule";
					break;
			}
			return WhatToDoNext.GoAhead;
		}

		[Option("Specifies the {name} of the Class or Module that contains Sub Main \tor inherits from System.Windows.Forms.Form.\tNeeded to select among many entry-points for a program (target=exe|winexe)",
			'm', "main")]
		public string MainClassName = null; 

		// TODO: force option to accept number in hex format
//		[Option("[NOT IMPLEMENTED YET]The base {address} for a library or module (hex)", SecondLevelHelp = true)]
		public int baseaddress;

		// input file options
		//------------------------------------------------------------------
		[Option(-1, "Imports all type information from files in the module-list. {module-list}:module,...", "addmodule")]
		public string AddedModule { set { foreach(string module in value.Split(',')) NetModulesToAdd.Add(module); } }

//		[Option("[NOT IMPLEMENTED YET]Include all files in the current directory and subdirectories according to the {wildcard}", "recurse")]
		public WhatToDoNext Recurse(string wildcard)
		{
			//AddFiles (DirName, true); // TODO wrong semantics
			return WhatToDoNext.GoAhead;
		}

		[Option(-1, "References metadata from the specified assembly-list. {assembly-list}:assembly,...", 'r', "reference")]
		public string AddedReference { set { foreach (string assembly in value.Split(',')) AssembliesToReference.Add(assembly); } }
		
		[Option("List of directories to search for referenced assemblies. \t{path-list}:path,...", "libpath", "lib")]
		public string AddedLibPath { set { foreach(string path in value.Split(',')) PathsToSearchForLibraries.Add(path); } }

		// support for the Compact Framework
		//------------------------------------------------------------------
//		[Option("[NOT IMPLEMENTED YET]Sets the compiler to TargetFileType the Compact Framework", "netcf")]
		public bool CompileForCompactFramework = false;
		
//		[Option("[NOT IMPLEMENTED YET]Specifies the {path} to the location of mscorlib.dll and microsoft.visualbasic.dll", "sdkpath")]
		public string SDKPath = null;

		// resource options
		//------------------------------------------------------------------
		public ArrayList EmbeddedResources = new ArrayList();
		
		//TODO: support -res:file[,id[,public|private]] what depends on changes at Mono.GetOptions
		[Option(-1, "Adds the specified file as an embedded assembly resource. \t{details}:file[,id[,public|private]]", "resource", "res")]
		public string AddedResource { set { EmbeddedResources.Add(value); } }

		public ArrayList LinkedResources = new ArrayList();
		
		[Option(-1, "Adds the specified file as a linked assembly resource. \t{details}:file[,id[,public|private]]", "linkresource", "linkres")]
		public string AddedLinkresource { set { LinkedResources.Add(value); } }

		public ArrayList Win32Resources = new ArrayList();
		
//		[Option(-1, "[NOT IMPLEMENTED YET]Specifies a Win32 resource {file} (.res)", "win32resource")]
		public string AddedWin32resource { set { Win32Resources.Add(value); } }

		public ArrayList Win32Icons = new ArrayList();
		
//		[Option(-1, "[NOT IMPLEMENTED YET]Specifies a Win32 icon {file} (.ico) for the default Win32 resources", "win32icon")]
		public string AddedWin32icon { set { Win32Icons.Add(value); } }

		// code generation options
		//------------------------------------------------------------------

//		[Option("[NOT IMPLEMENTED YET]Enable optimizations", "optimize", VBCStyleBoolean = true)]
		public bool Optimize = false;

		public bool CheckedContext = true;
		
		[Option("Remove integer checks. Default off.", SecondLevelHelp = true, VBCStyleBoolean = true)]
		public virtual bool removeintchecks { set { CheckedContext = !value; } }

		[Option("Emit full debugging information", 'g', "debug", VBCStyleBoolean = true)]
		public bool WantDebuggingSupport = false;

		[Option("Emit full debugging information (default)", "debug:full", SecondLevelHelp = true)]
		public bool debugfull { 
			set { 
				WantDebuggingSupport = value; 
				FullDebugging = value; 
				MdbOnly = !value; 
			}
		}
		
		[Option("Emit MDB file only", "debug:pdbonly", SecondLevelHelp = true)]
		public bool debugpdbonly {
			set { 
				WantDebuggingSupport = value; 
				FullDebugging = !value; 
				MdbOnly = value; 
			}
		}
	
		public bool MdbOnly = false;
		public bool FullDebugging = true;


		// errors and warnings options
		//------------------------------------------------------------------

		[Option("Treat warnings as errors", "warnaserror", SecondLevelHelp = true)]
		public bool WarningsAreErrors = false; 

		[Option("Disable warnings", "nowarn", SecondLevelHelp = true)]
		public bool NoWarnings { set { if (value) WarningLevel = 0; } }


		// Defines
		//------------------------------------------------------------------
		public Hashtable Defines = new Hashtable();
		
		[Option(-1, "Declares global conditional compilation symbol(s). {symbol-list}:name=value,...", 'd', "define")]
		public string DefineSymbol { 
			set {
				foreach(string item in value.Split(','))  {
					string[] dados = item.Split('=');
					if (dados.Length > 1)
						Defines.Add(dados[0], dados[1]); 
					else
						Defines.Add(dados[0], "true");
				}
			} 
		}
		
		[Option("Don\'t assume the standard library", "nostdlib", SecondLevelHelp = true)]
		public bool NoStandardLibraries = false;

		[Option("Disables implicit references to assemblies", "noconfig", SecondLevelHelp = true)]
		public bool NoConfig = false;
		
		[Option("Allows unsafe code", "unsafe", SecondLevelHelp = true)]
		public bool AllowUnsafeCode = false;

		[Option("Debugger {arguments}", "debug-args", SecondLevelHelp = true)]
		public WhatToDoNext SetDebugArgs(string args)
		{
			DebugListOfArguments.AddRange(args.Split(','));
			return WhatToDoNext.GoAhead;
		}

		public ArrayList Imports = new ArrayList();
		
		[Option(-1, "Declare global Imports for listed namespaces. {import-list}:namespace,...", "imports")]
		public string ImportNamespaces
		{
			set {
				foreach(string importedNamespace in value.Split(','))
					Imports.Add(importedNamespace);
			}
		}

		[Option("Specifies the root {namespace} for all type declarations", "rootnamespace",  SecondLevelHelp = true)]
		public string RootNamespace = null;
		
		// Signing options	
		//------------------------------------------------------------------
//		[Option("[NOT IMPLEMENTED YET]Delay-sign the assembly using only the public portion of the strong name key", VBCStyleBoolean = true)]
		public bool delaysign;
		
//		[Option("[NOT IMPLEMENTED YET]Specifies a strong name key {container}")]
		public string keycontainer;
		
//		[Option("[NOT IMPLEMENTED YET]Specifies a strong name key {file}")]
		public string keyfile;

		// Compiler output options	
		//------------------------------------------------------------------
		
		[Option("Do not display compiler copyright banner", "nologo")]
		public bool DontShowBanner = false;
		
		//TODO: Correct semantics
		[Option("Commands the compiler to show only error messages for syntax-related errors and warnings", 'q', "quiet", SecondLevelHelp = true)]
		public bool SuccintErrorDisplay = false;
		
		[Option("Display verbose messages", 'v', "verbose",  SecondLevelHelp = true)] 
		public bool Verbose = false;
		
		[Option("[IGNORED] Emit compiler output in UTF8 character encoding", "utf8output", SecondLevelHelp = true, VBCStyleBoolean = true)]
		public bool OutputInUTF8;

//		[Option("[NOT IMPLEMENTED YET]Create bug report {file}", "bugreport")]
		public string CreateBugReport;

		Hashtable sourceFiles = new Hashtable ();
		public override void DefaultArgumentProcessor(string fileName)
		{
			if (firstSourceFile == null)
				firstSourceFile = fileName;

			if (!sourceFiles.Contains(fileName)) {
				SourceFilesToCompile.Add(new FileToCompile(fileName, currentEncoding));
				sourceFiles.Add(fileName, fileName);
			}
			base.DefaultArgumentProcessor(fileName);
		}		

		public ArrayList AssembliesToReference = new ArrayList();
		public ArrayList NetModulesToAdd = new ArrayList();
		public ArrayList PathsToSearchForLibraries = new ArrayList();
		public ArrayList DebugListOfArguments = new ArrayList ();
		public ArrayList SourceFilesToCompile = new ArrayList();
		
		public bool ReferencePackage(string packageName)
		{
			if (packageName == ""){
				DoAbout ();
				return false;
			}
				
			ProcessStartInfo pi = new ProcessStartInfo ();
			pi.FileName = "pkg-config";
			pi.RedirectStandardOutput = true;
			pi.UseShellExecute = false;
			pi.Arguments = "--libs " + packageName;
			Process p = null;
			try {
				p = Process.Start (pi);
			} catch (Exception e) {
				ReportError (0, "Couldn't run pkg-config: " + e.Message);
				return false;
			}

			if (p.StandardOutput == null){
				ReportError (0, "Specified package did not return any information");
			}
			string pkgout = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();
			if (p.ExitCode != 0) {
				ReportError (0, "Error running pkg-config. Check the above output.");
				return false;
			}
			p.Close ();
			
			if (pkgout != null) {
				string [] xargs = pkgout.Trim (new Char [] {' ', '\n', '\r', '\t'}).
					Split (new Char [] { ' ', '\t'});
				foreach(string arg in xargs) {
					string[] zargs = arg.Split(':', '=');
					try {
						if (zargs.Length > 1)
							AddedReference = zargs[1];
						else
							AddedReference = arg;
					} catch (Exception e) {
						ReportError (0, "Something wrong with argument (" + arg + ") in 'pkg-config --libs' output: " + e.Message);
						return false;
					}
				}
			}

			return true;
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
		
		[Option("Displays time stamps of various compiler events", "timestamp", SecondLevelHelp = true)]
		public virtual bool PrintTimeStamps {
			set
			{
				printTimeStamps = true;
				last_time = DateTime.Now;
				DebugListOfArguments.Add("timestamp");
			}
		}

		public bool BeQuiet { get { return DontShowBanner || SuccintErrorDisplay; } } 
		
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
				}
				adder(a);
				return;
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
					
				ReportError (6, "Can not find assembly '" + assemblyName + "'\nLog: " + total_log);
			}
			catch (BadImageFormatException f)  {
				ReportError (6, "Bad file format while loading assembly\nLog: " + f.FusionLog);
			} catch (FileLoadException f){
				ReportError (6, "File Load Exception: " + assemblyName + "\nLog: " + f.FusionLog);
			} catch (ArgumentNullException){
				ReportError (6, "Argument Null exception");
			}
						
			errors++;
		}
		
		public virtual string [] AssembliesToReferenceSoftly {
			get {
				// For now the "default config" is hardcoded we can move this outside later
				return new string [] { "System", "System.Data", "System.Xml" };
			}
		}
		
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
				ReportError (6, "Cannot find module `" + module + "'" );
				Console.WriteLine ("Log: \n" + total_log);
			}
			catch (BadImageFormatException f) {
				ReportError (6, "Cannot load module (bad file format)" + f.FusionLog);
			}
			catch (FileLoadException f)	{
				ReportError (6, "Cannot load module " + f.FusionLog);
			}
			catch (ArgumentNullException) {
				ReportError (6, "Cannot load module (null argument)");
			}
			errors++;
		}

		public void UnsupportedFeatureOnthisRuntime(string feature)
		{
			ReportError (0, string.Format("Cannot use {0} on this runtime: Try the Mono runtime instead.", feature));
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
				ReportError (2001, "Source file '" + spec + "' could not be found");
				return false;
			} catch (System.IO.IOException){
				ReportError (2001, "Source file '" + spec + "' could not be found");
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

		public virtual bool NothingToCompile {
			get {
				if (SourceFilesToCompile.Count == 0) {
					if (!BeQuiet) 
						DoHelp();
					return true;
				}
				if (!BeQuiet)
					ShowBanner();
				return false;
			}
		}

	}
	
	public class CommonCompilerOptions2 : CommonCompilerOptions
	{
		[Option("Specify target CPU platform {ID}. ID can be x86, Itanium, x64 (AMD 64bit) or anycpu (the default).", "platform", SecondLevelHelp = true)]
		public string TargetPlatform;
		
		[Option("What {action} (prompt | send | none) should be done when an internal compiler error occurs.\tThe default is none what just prints the error data in the compiler output", "errorreport", SecondLevelHelp = true)]
		public InternalCompilerErrorReportAction HowToReportErrors = InternalCompilerErrorReportAction.none;
		
		[Option("Filealign internal blocks to the {blocksize} in bytes. Valid values are 512, 1024, 2048, 4096, and 8192.", "filealign", SecondLevelHelp = true)]
		public int FileAlignBlockSize = 0; // 0 means use appropriate (not fixed) default		
		
		[Option("Generate documentation from xml commments.", "doc", SecondLevelHelp = true, VBCStyleBoolean = true)]
		public bool GenerateXmlDocumentation = false;
		
		[Option("Generate documentation from xml commments to an specific {file}.", "docto", SecondLevelHelp = true)]
		public string GenerateXmlDocumentationToFileName = null;
	}
	
}
