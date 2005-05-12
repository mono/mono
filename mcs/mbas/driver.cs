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

/*

*/

namespace Mono.Languages {

	using System;
	using System.Collections;
	using System.IO;
	using System.Text;
	using System.Globalization;
	using System.Reflection;
	using System.Reflection.Emit;

	using Mono.MonoBASIC;
	using Mono.GetOptions.Useful;

	public class Driver {
		CompilerOptions options;
		
		private void InitializeRootContextAndOthersFromOptions()
		{
			Report.Stacktrace = options.Stacktrace;
			Report.WarningsAreErrors = options.WarningsAreErrors;
			// TODO: change Report to receive the whole array
			for(int i = 0; i < options.WarningsToIgnore.Length; i++)
				Report.SetIgnoreWarning(options.WarningsToIgnore[i]);
			Report.Fatal = options.MakeErrorsFatal;

			RootContext.WarningLevel = options.WarningLevel;
			RootContext.Checked = options.CheckedContext;
			RootContext.MainClass = options.MainClassName;
			RootContext.StdLib = !options.NoStandardLibraries;
			RootContext.Unsafe = options.AllowUnsafeCode;
			if (options.RootNamespace != null)
				RootContext.RootNamespace = options.RootNamespace;
			
			// TODO: semantics are different and should be adjusted
			GenericParser.yacc_verbose_flag = options.Verbose ? 1 : 0;
			
			Mono.MonoBASIC.Parser.InitialOptionExplicit = options.OptionExplicit;
			Mono.MonoBASIC.Parser.InitialOptionStrict = options.OptionStrict;
		    Mono.MonoBASIC.Parser.InitialOptionCompareBinary = options.OptionCompareBinary;
		    Mono.MonoBASIC.Parser.ImportsList = options.Imports;
		}
		
		bool ParseAllSourceFiles()
		{
			options.StartTime("Parsing Source Files");
			foreach(FileToCompile file in options.SourceFilesToCompile)
				GenericParser.Parse(file.Filename, file.Encoding);
			options.ShowTime("   Done");
			return (Report.Errors == 0);
		}
		
		private bool InitializeDebuggingSupport()
		{
			string[] debug_args = new string [options.DebugListOfArguments.Count];
			options.DebugListOfArguments.CopyTo(debug_args);
			CodeGen.Init(options.OutputFileName, options.OutputFileName, options.WantDebuggingSupport, debug_args);
			TypeManager.AddModule(CodeGen.ModuleBuilder);
			return true;
		}
		
		private bool LoadReferencedAssemblies()
		{
			return options.LoadReferencedAssemblies(TypeManager.AddAssembly);
		}

		private bool AdjustCodegenWhenTargetIsNetModule()
		{
			options.AdjustCodegenWhenTargetIsNetModule(CodeGen.AssemblyBuilder);
			return true;
		}
		
		private bool LoadAddedNetModules()
		{
			return options.LoadAddedNetModules(CodeGen.AssemblyBuilder, TypeManager.AddModule);
		}
		
		private bool InitializeCoreTypes()
		{
			//
			// Before emitting, we need to get the core
			// types emitted from the user defined types
			// or from the system ones.
			//
			options.StartTime("Initializing Core Types");

			if (!RootContext.StdLib)
				RootContext.ResolveCore ();
			if (Report.Errors > 0)
				return false;
			
			TypeManager.InitCoreTypes();
				
			options.ShowTime("Core Types Done");
			return Report.Errors == 0;
		}

		private bool ResolveTree()
		{
			options.StartTime("Resolving tree");
			RootContext.ResolveTree (); // The second pass of the compiler
			options.ShowTime("Tree resolved");
			return Report.Errors == 0;
		}
		
		private bool PopulateCoreTypes()
		{
			if (!RootContext.StdLib) {
				options.StartTime("Populate core types");
				RootContext.BootCorlib_PopulateCoreTypes();
				options.ShowTime("   Done");
			}
			return Report.Errors == 0;
		}
						
		private bool PopulateTypes()
		{
			options.StartTime("Populate types");
			RootContext.PopulateTypes();
			options.ShowTime("   Done");
			return Report.Errors == 0;
		}
						
		private bool InitCodeHelpers()
		{
			options.StartTime("Initialize code helpers");
			TypeManager.InitCodeHelpers();
			options.ShowTime("   Done");
			return Report.Errors == 0;
		}
						
		string GetFQMainClass()
		{	
			if (RootContext.RootNamespace != "")
				return RootContext.RootNamespace + "." + RootContext.MainClass;
			else
				return RootContext.MainClass;			
		}
		
		bool IsSWFApp()
		{
			string mainclass = GetFQMainClass();
			
			if (mainclass != null) {
				foreach (string r in options.AssembliesToReference) {
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
		
		void FixEntryPoint()
		{
			if (options.TargetFileType == TargetType.Exe || options.TargetFileType == TargetType.WinExe) {
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
		
		private bool EmitCode()
		{
			options.StartTime("Emitting code");			
			RootContext.EmitCode();
			FixEntryPoint();
			options.ShowTime("   Done");
			return Report.Errors == 0;
		}

		private bool CloseTypes()
		{
			options.StartTime("Closing types");			
			RootContext.CloseTypes ();
			options.ShowTime("   Done");
			return Report.Errors == 0;
		}

		private bool SetEntryPoint()
		{
			if (options.TargetFileType == TargetType.Exe || options.TargetFileType == TargetType.WinExe) {
				options.StartTime("Setting entry point");			
				MethodInfo ep = RootContext.EntryPoint;
			
				if (ep == null) {
					Report.Error (30737, "Program " + options.OutputFileName + " does not have an entry point defined");
					return false;
				}
							
				CodeGen.AssemblyBuilder.SetEntryPoint (ep, 
					(options.TargetFileType == TargetType.Exe)?PEFileKinds.ConsoleApplication:PEFileKinds.WindowApplication);
				options.ShowTime("   Done");
			}
			return Report.Errors == 0;
		}

		private bool EmbedResources()
		{
			options.StartTime("Embedding resources");			
			options.EmbedResources(CodeGen.AssemblyBuilder);
			options.ShowTime("   Done");
			return Report.Errors == 0;
		}

		private bool SaveOutput()
		{
			options.StartTime("Saving Output");			
			CodeGen.Save(options.OutputFileName);
			options.ShowTime("   Done");
			return Report.Errors == 0;
		}


		private bool SaveDebugSymbols()
		{
			if (options.WantDebuggingSupport)  {
				options.StartTime("Saving Debug Symbols");			
				CodeGen.SaveSymbols ();
				options.ShowTime ("   Done");
			}
			return true;
		}

		delegate bool CompilerStep();
		
		private CompilerStep[] Steps {
			get {
				return new CompilerStep[] {
					ParseAllSourceFiles,
					InitializeDebuggingSupport,
					LoadReferencedAssemblies,
					AdjustCodegenWhenTargetIsNetModule,
					LoadAddedNetModules,
					InitializeCoreTypes,
					ResolveTree,
					PopulateCoreTypes,
					PopulateTypes,
					InitCodeHelpers,
					EmitCode,
					CloseTypes,
					SetEntryPoint,
					SaveOutput,
					SaveDebugSymbols } ;
			}
		}

		/// <summary>
		///    Parses the arguments, and calls the compilation process.
		/// </summary>
		int MainDriver(string [] args)
		{
			options = new CompilerOptions(args, Report.Error);		
			if (options.NothingToCompile)
				return 2;		
			try {
				InitializeRootContextAndOthersFromOptions();
				
				foreach(CompilerStep step in Steps)
					if (!step())
						break;
						
			} catch (Exception ex) {
				Report.Error(0, "Exception: " + ex.ToString());
			}
			return Report.ProcessResults(options.BeQuiet);
		}

		public static int Main (string[] args)
		{
			Driver Exec = new Driver();
			
			return Exec.MainDriver(args);
		}


	}
}
