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
using System.Diagnostics;
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
	
	public class CommonCompilerOptions : Options {
	
		public CommonCompilerOptions() : this(null) { }

		public CommonCompilerOptions(string[] args) : base(args, OptionsParsingMode.Both, false, true, true) {}
		
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
							Console.WriteLine("Ignoring unsupported codepage number {0}.", value);
						} catch (Exception) {
							Console.WriteLine("Ignoring unsupported codepage ID {0}.", value);
						}
						break;
				}					
			}
		}
		
		private ArrayList warningsToIgnore = new ArrayList();
		public int[] WarningsToIgnore { get { return (int[])warningsToIgnore.ToArray(typeof(int)); } }
		
		[Option("Ignores warning number {XXXX}", "ignorewarn", SecondLevelHelp = true)]
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

		[Option("Specifies the {name} of the Class or Module that contains Sub Main or inherits from System.Windows.Forms.Form.\tNeeded to select among many entry-points for a program (target=exe|winexe)",
			'm', "main")]
		public string MainClassName = null; 

		// TODO: force option to accept number in hex format
//		[Option("[NOT IMPLEMENTED YET]The base {address} for a library or module (hex)", SecondLevelHelp = true)]
		public int baseaddress;

		// input file options
		//------------------------------------------------------------------
		[Option(-1, "Imports all type information from files in the {module-list}. {module-list}:module,...", "addmodule")]
		public string AddedModule { set { foreach(string module in value.Split(',')) NetModulesToAdd.Add(module); } }

//		[Option("[NOT IMPLEMENTED YET]Include all files in the current directory and subdirectories according to the {wildcard}", "recurse")]
		public WhatToDoNext Recurse(string wildcard)
		{
			//AddFiles (DirName, true); // TODO wrong semantics
			return WhatToDoNext.GoAhead;
		}

		[Option(-1, "References metadata from the specified {assembly-list}. {assembly-list}:assembly,...", 'r', "reference")]
		public string AddedReference { set { foreach (string assembly in value.Split(',')) AssembliesToReference.Add(assembly); } }
		
		[Option("List of directories to search for metadata AssembliesToReference. {path-list}:path,...", "libpath", "lib")]
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
		[Option(-1, "Adds the specified file as an embedded assembly resource. {details}:file[,id[,public|private]]", "resource", "res")]
		public string AddedResource { set { EmbeddedResources.Add(value); } }

		public ArrayList LinkedResources = new ArrayList();
		
//		[Option(-1, "[NOT IMPLEMENTED YET]Adds the specified {file[,id]} as a linked assembly resource", "linkresource", "linkres")]
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

		[Option("Emit debugging information", 'g', "debug", VBCStyleBoolean = true)]
		public bool WantDebuggingSupport = false;

		[Option("Emit full debugging information (default)", "debug:full", SecondLevelHelp = true)]
		public bool FullDebugging = false;

		[Option("[IGNORED] Emit PDB file only", "debug:pdbonly", SecondLevelHelp = true)]
		public bool MdbOnly = false;

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
				Console.WriteLine("Couldn't run pkg-config: " + e.Message);
				return false;
			}

			if (p.StandardOutput == null){
				Console.WriteLine("Specified package did not return any information");
			}
			string pkgout = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();
			if (p.ExitCode != 0) {
				Console.WriteLine("Error running pkg-config. Check the above output.");
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
						Console.WriteLine("Something wrong with argument (" + arg + ") in 'pkg-config --libs' output: " + e.Message);
						return false;
					}
				}
			}

			return true;
		}		
	}

}
