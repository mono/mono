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
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

	using Mono.GetOptions;
	using Mono.GetOptions.Useful;

	public enum OptionCompare {
		Binary, Text
	};

	/// <summary>
	///    The compiler command line options processor.
	/// </summary>
	public class CompilerOptions : CommonCompilerOptions {
	
		public override string [] AssembliesToReferenceSoftly {
			get {
				//
				// For now the "default config" is hardcoded into the compiler
				// we can move this outside later
				//
				return new string [] {
					"System",
					"System.Data",
					"System.Xml",
					"Microsoft.VisualBasic" 
#if EXTRA_DEFAULT_REFS
					//
					// Is it worth pre-loading all this stuff?
					//
					,
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
			} 
		}

		public CompilerOptions(string [] args, ErrorReporter ReportError) : base(args, ReportError) {}
		
		protected override void InitializeOtherDefaults() 
		{ 
			DefineSymbol = "__MonoBASIC__";
			ImportNamespaces = "Microsoft.VisualBasic";
		}
		
		// TODO: remove next lines when the compler has matured enough
		public override string AdditionalBannerInfo { get { return "--------\nTHIS IS AN ALPHA SOFTWARE.\n--------"; } }

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
		
	}
}
