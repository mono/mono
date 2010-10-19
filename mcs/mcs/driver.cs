//
// driver.cs: The compiler command line driver.
//
// Authors:
//   Miguel de Icaza (miguel@gnu.org)
//   Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004, 2005, 2006, 2007, 2008 Novell, Inc
//

namespace Mono.CSharp
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Globalization;
	using System.Diagnostics;

	public enum Target {
		Library, Exe, Module, WinExe
	};

	public enum Platform {
		AnyCPU, X86, X64, IA64
	}
	
	/// <summary>
	///    The compiler driver.
	/// </summary>
	class Driver
	{
		//
		// Assemblies references to be linked.   Initialized with
		// mscorlib.dll here.
		List<string> references;

		//
		// If any of these fail, we ignore the problem.  This is so
		// that we can list all the assemblies in Windows and not fail
		// if they are missing on Linux.
		//
		List<string> soft_references;

		// 
		// External aliases for assemblies.
		//
		Dictionary<string, string> external_aliases;

		//
		// Modules to be linked
		//
		List<string> modules;

		// Lookup paths
		List<string> link_paths;

		// Whether we want to only run the tokenizer
		bool tokenize;
		
		string first_source;

		bool want_debugging_support;
		bool parse_only;
		bool timestamps;
		internal int fatal_errors;
		
		//
		// Whether to load the initial config file (what CSC.RSP has by default)
		// 
		bool load_default_config = true;

		//
		// A list of resource files
		//
		Resources embedded_resources;
		string win32ResourceFile;
		string win32IconFile;

		//
		// Output file
		//
		static string output_file;

		//
		// Last time we took the time
		//
		DateTime last_time, first_time;

		//
		// Encoding.
		//
		Encoding encoding;

		internal readonly CompilerContext ctx;

		static readonly char[] argument_value_separator = new char [] { ';', ',' };

		static public void Reset ()
		{
			output_file = null;
		}

		private Driver (CompilerContext ctx)
		{
			this.ctx = ctx;
			encoding = Encoding.Default;
		}

		public static Driver Create (string[] args, bool require_files, ReportPrinter printer)
		{
			Driver d = new Driver (new CompilerContext (new ReflectionMetaImporter (), new Report (printer)));

			if (!d.ParseArguments (args, require_files))
				return null;

			return d;
		}

		Report Report {
			get { return ctx.Report; }
		}

		void ShowTime (string msg)
		{
			if (!timestamps)
				return;

			DateTime now = DateTime.Now;
			TimeSpan span = now - last_time;
			last_time = now;

			Console.WriteLine (
				"[{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}

		void ShowTotalTime (string msg)
		{
			if (!timestamps)
				return;

			DateTime now = DateTime.Now;
			TimeSpan span = now - first_time;
			last_time = now;

			Console.WriteLine (
				"[{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}	       
	       
		void tokenize_file (CompilationUnit file, CompilerContext ctx)
		{
			Stream input;

			try {
				input = File.OpenRead (file.Name);
			} catch {
				Report.Error (2001, "Source file `" + file.Name + "' could not be found");
				return;
			}

			using (input){
				SeekableStreamReader reader = new SeekableStreamReader (input, encoding);
				Tokenizer lexer = new Tokenizer (reader, file, ctx);
				int token, tokens = 0, errors = 0;

				while ((token = lexer.token ()) != Token.EOF){
					tokens++;
					if (token == Token.ERROR)
						errors++;
				}
				Console.WriteLine ("Tokenized: " + tokens + " found " + errors + " errors");
			}
			
			return;
		}

		void Parse (CompilationUnit file)
		{
			Stream input;

			try {
				input = File.OpenRead (file.Name);
			} catch {
				Report.Error (2001, "Source file `{0}' could not be found", file.Name);
				return;
			}

			SeekableStreamReader reader = new SeekableStreamReader (input, encoding);

			// Check 'MZ' header
			if (reader.Read () == 77 && reader.Read () == 90) {
				Report.Error (2015, "Source file `{0}' is a binary file and not a text file", file.Name);
				input.Close ();
				return;
			}

			reader.Position = 0;
			Parse (reader, file);
			reader.Dispose ();
			input.Close ();
		}	
		
		void Parse (SeekableStreamReader reader, CompilationUnit file)
		{
			CSharpParser parser = new CSharpParser (reader, file, ctx);
			parser.parse ();
		}

		static void OtherFlags ()
		{
			Console.WriteLine (
				"Other flags in the compiler\n" +
				"   --fatal[=COUNT]    Makes errors after COUNT fatal\n" +
				"   --lint             Enhanced warnings\n" +
				"   --parse            Only parses the source file\n" +
				"   --stacktrace       Shows stack trace at error location\n" +
				"   --timestamp        Displays time stamps of various compiler events\n" +
				"   -v                 Verbose parsing (for debugging the parser)\n" + 
				"   --mcs-debug X      Sets MCS debugging level to X\n");
		}
		
		static void Usage ()
		{
			Console.WriteLine (
				"Mono C# compiler, Copyright 2001 - 2008 Novell, Inc.\n" +
				"mcs [options] source-files\n" +
				"   --about              About the Mono C# compiler\n" +
				"   -addmodule:M1[,Mn]   Adds the module to the generated assembly\n" + 
				"   -checked[+|-]        Sets default aritmetic overflow context\n" +
				"   -codepage:ID         Sets code page to the one in ID (number, utf8, reset)\n" +
				"   -clscheck[+|-]       Disables CLS Compliance verifications\n" +
				"   -define:S1[;S2]      Defines one or more conditional symbols (short: -d)\n" +
				"   -debug[+|-], -g      Generate debugging information\n" + 
				"   -delaysign[+|-]      Only insert the public key into the assembly (no signing)\n" +
				"   -doc:FILE            Process documentation comments to XML file\n" + 
				"   -help                Lists all compiler options (short: -?)\n" + 
				"   -keycontainer:NAME   The key pair container used to sign the output assembly\n" +
				"   -keyfile:FILE        The key file used to strongname the ouput assembly\n" +
				"   -langversion:TEXT    Specifies language version: ISO-1, ISO-2, Default, or Future\n" + 
				"   -lib:PATH1[,PATHn]   Specifies the location of referenced assemblies\n" +
				"   -main:CLASS          Specifies the class with the Main method (short: -m)\n" +
				"   -noconfig            Disables implicitly referenced assemblies\n" +
				"   -nostdlib[+|-]       Does not reference mscorlib.dll library\n" +
				"   -nowarn:W1[,Wn]      Suppress one or more compiler warnings\n" + 
				"   -optimize[+|-]       Enables advanced compiler optimizations (short: -o)\n" + 
				"   -out:FILE            Specifies output assembly name\n" +
#if !SMCS_SOURCE
				"   -pkg:P1[,Pn]         References packages P1..Pn\n" + 
#endif
				"   -platform:ARCH       Specifies the target platform of the output assembly\n" +
				"                        ARCH can be one of: anycpu, x86, x64 or itanium\n" +
				"   -recurse:SPEC        Recursively compiles files according to SPEC pattern\n" + 
				"   -reference:A1[,An]   Imports metadata from the specified assembly (short: -r)\n" +
				"   -reference:ALIAS=A   Imports metadata using specified extern alias (short: -r)\n" +				
				"   -target:KIND         Specifies the format of the output assembly (short: -t)\n" +
				"                        KIND can be one of: exe, winexe, library, module\n" +
				"   -unsafe[+|-]         Allows to compile code which uses unsafe keyword\n" +
				"   -warnaserror[+|-]    Treats all warnings as errors\n" +
				"   -warnaserror[+|-]:W1[,Wn] Treats one or more compiler warnings as errors\n" +
				"   -warn:0-4            Sets warning level, the default is 4 (short -w:)\n" +
				"   -help2               Shows internal compiler options\n" + 
				"\n" +
				"Resources:\n" +
				"   -linkresource:FILE[,ID] Links FILE as a resource (short: -linkres)\n" +
				"   -resource:FILE[,ID]     Embed FILE as a resource (short: -res)\n" +
				"   -win32res:FILE          Specifies Win32 resource file (.res)\n" +
				"   -win32icon:FILE         Use this icon for the output\n" +
                                "   @file                   Read response file for more options\n\n" +
				"Options can be of the form -option or /option");
		}

		void TargetUsage ()
		{
			Report.Error (2019, "Invalid target type for -target. Valid options are `exe', `winexe', `library' or `module'");
		}
		
		static void About ()
		{
			Console.WriteLine (
				"The Mono C# compiler is Copyright 2001-2008, Novell, Inc.\n\n" +
				"The compiler source code is released under the terms of the \n"+
				"MIT X11 or GNU GPL licenses\n\n" +

				"For more information on Mono, visit the project Web site\n" +
				"   http://www.mono-project.com\n\n" +

				"The compiler was written by Miguel de Icaza, Ravi Pratap, Martin Baulig, Marek Safar, Raja R Harinath, Atushi Enomoto");
			Environment.Exit (0);
		}

		public static int Main (string[] args)
		{
			Location.InEmacs = Environment.GetEnvironmentVariable ("EMACS") == "t";
			var crp = new ConsoleReportPrinter ();
			Driver d = Driver.Create (args, true, crp);
			if (d == null)
				return 1;

			crp.Fatal = d.fatal_errors;

			if (d.Compile () && d.Report.Errors == 0) {
				if (d.Report.Warnings > 0) {
					Console.WriteLine ("Compilation succeeded - {0} warning(s)", d.Report.Warnings);
				}
				Environment.Exit (0);
				return 0;
			}
			
			
			Console.WriteLine("Compilation failed: {0} error(s), {1} warnings",
				d.Report.Errors, d.Report.Warnings);
			Environment.Exit (1);
			return 1;
		}

		public void LoadAssembly (string assembly, bool soft)
		{
			LoadAssembly (assembly, null, soft);
		}

		void Error6 (string name, string log)
		{
			if (log != null && log.Length > 0)
				Report.ExtraInformation (Location.Null, "Log:\n" + log + "\n(log related to previous ");
			Report.Error (6, "cannot find metadata file `{0}'", name);
		}

		void Error9 (string type, string filename, string log)
		{
			if (log != null && log.Length > 0)
				Report.ExtraInformation (Location.Null, "Log:\n" + log + "\n(log related to previous ");
			Report.Error (9, "file `{0}' has invalid `{1}' metadata", filename, type);
		}

		void BadAssembly (string filename, string log)
		{
			MethodInfo adder_method = AssemblyClass.AddModule_Method;

			if (adder_method != null) {
				AssemblyName an = new AssemblyName ();
				an.Name = ".temp";
				AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
				try {
					object m = null;
					try {
						m = adder_method.Invoke (ab, new object [] { filename });
					} catch (TargetInvocationException ex) {
						throw ex.InnerException;
					}

					if (m != null) {
						Report.Error (1509, "Referenced file `{0}' is not an assembly. Consider using `-addmodule' option instead",
						              Path.GetFileName (filename));
						return;
					}
				} catch (FileNotFoundException) {
					// did the file get deleted during compilation? who cares? swallow the exception
				} catch (BadImageFormatException) {
					// swallow exception
				} catch (FileLoadException) {
					// swallow exception
				}
			}
			Error9 ("assembly", filename, log);
		}

		public void LoadAssembly (string assembly, string alias, bool soft)
		{
			Assembly a = null;
			string total_log = "";

			try {
				try {
					char[] path_chars = { '/', '\\' };

					if (assembly.IndexOfAny (path_chars) != -1) {
						a = Assembly.LoadFrom (assembly);
					} else {
						string ass = assembly;
						if (ass.EndsWith (".dll") || ass.EndsWith (".exe"))
							ass = assembly.Substring (0, assembly.Length - 4);
						a = Assembly.Load (ass);
					}
				} catch (FileNotFoundException) {
					bool err = !soft;
					foreach (string dir in link_paths) {
						string full_path = Path.Combine (dir, assembly);
						if (!assembly.EndsWith (".dll") && !assembly.EndsWith (".exe"))
							full_path += ".dll";

						try {
							a = Assembly.LoadFrom (full_path);
							err = false;
							break;
						} catch (FileNotFoundException ff) {
							if (soft)
								return;
							total_log += ff.FusionLog;
						}
					}
					if (err) {
						Error6 (assembly, total_log);
						return;
					}
				}

				// Extern aliased refs require special handling
				if (alias == null)
					ctx.GlobalRootNamespace.AddAssemblyReference (a);
				else
					ctx.GlobalRootNamespace.DefineRootNamespace (alias, a, ctx);

			} catch (BadImageFormatException f) {
				// .NET 2.0 throws this if we try to load a module without an assembly manifest ...
				BadAssembly (f.FileName, f.FusionLog);
			} catch (FileLoadException f) {
				// ... while .NET 1.1 throws this
				BadAssembly (f.FileName, f.FusionLog);
			}
		}

		public void LoadModule (string module)
		{
			Module m = null;
			string total_log = "";

			try {
				try {
					m = CodeGen.Assembly.AddModule (module);
				} catch (FileNotFoundException) {
					bool err = true;
					foreach (string dir in link_paths) {
						string full_path = Path.Combine (dir, module);
						if (!module.EndsWith (".netmodule"))
							full_path += ".netmodule";

						try {
							m = CodeGen.Assembly.AddModule (full_path);
							err = false;
							break;
						} catch (FileNotFoundException ff) {
							total_log += ff.FusionLog;
						}
					}
					if (err) {
						Error6 (module, total_log);
						return;
					}
				}

				ctx.GlobalRootNamespace.AddModuleReference (m);

			} catch (BadImageFormatException f) {
				Error9 ("module", f.FileName, f.FusionLog);
			} catch (FileLoadException f) {
				Error9 ("module", f.FileName, f.FusionLog);
			}
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		public void LoadReferences ()
		{
			link_paths.Add (GetSystemDir ());
			link_paths.Add (Directory.GetCurrentDirectory ());

			//
			// Load Core Library for default compilation
			//
			if (RootContext.StdLib)
				LoadAssembly ("mscorlib", false);

			foreach (string r in soft_references)
				LoadAssembly (r, true);

			foreach (string r in references)
				LoadAssembly (r, false);

			foreach (var entry in external_aliases)
				LoadAssembly (entry.Value, entry.Key, false);

			if (modules.Count > 0) {
				foreach (string module in modules)
					LoadModule (module);
			}
				
			ctx.GlobalRootNamespace.ComputeNamespaces (ctx);
		}

		static string [] LoadArgs (string file)
		{
			StreamReader f;
			var args = new List<string> ();
			string line;
			try {
				f = new StreamReader (file);
			} catch {
				return null;
			}

			StringBuilder sb = new StringBuilder ();
			
			while ((line = f.ReadLine ()) != null){
				int t = line.Length;

				for (int i = 0; i < t; i++){
					char c = line [i];
					
					if (c == '"' || c == '\''){
						char end = c;
						
						for (i++; i < t; i++){
							c = line [i];

							if (c == end)
								break;
							sb.Append (c);
						}
					} else if (c == ' '){
						if (sb.Length > 0){
							args.Add (sb.ToString ());
							sb.Length = 0;
						}
					} else
						sb.Append (c);
				}
				if (sb.Length > 0){
					args.Add (sb.ToString ());
					sb.Length = 0;
				}
			}

			return args.ToArray ();
		}

		//
		// Returns the directory where the system assemblies are installed
		//
		static string GetSystemDir ()
		{
			return Path.GetDirectoryName (typeof (object).Assembly.Location);
		}

		//
		// Given a path specification, splits the path from the file/pattern
		//
		static void SplitPathAndPattern (string spec, out string path, out string pattern)
		{
			int p = spec.LastIndexOf ('/');
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

			p = spec.LastIndexOf ('\\');
			if (p != -1){
				path = spec.Substring (0, p);
				pattern = spec.Substring (p + 1);
				return;
			}

			path = ".";
			pattern = spec;
		}

		void AddSourceFile (string f)
		{
			if (first_source == null)
				first_source = f;

			Location.AddFile (Report, f);
		}

		bool ParseArguments (string[] args, bool require_files)
		{
			references = new List<string> ();
			external_aliases = new Dictionary<string, string> ();
			soft_references = new List<string> ();
			modules = new List<string> (2);
			link_paths = new List<string> ();

			List<string> response_file_list = null;
			bool parsing_options = true;

			for (int i = 0; i < args.Length; i++) {
				string arg = args [i];
				if (arg.Length == 0)
					continue;

				if (arg [0] == '@') {
					string [] extra_args;
					string response_file = arg.Substring (1);

					if (response_file_list == null)
						response_file_list = new List<string> ();

					if (response_file_list.Contains (response_file)) {
						Report.Error (
							1515, "Response file `" + response_file +
							"' specified multiple times");
						return false;
					}

					response_file_list.Add (response_file);

					extra_args = LoadArgs (response_file);
					if (extra_args == null) {
						Report.Error (2011, "Unable to open response file: " +
								  response_file);
						return false;
					}

					args = AddArgs (args, extra_args);
					continue;
				}

				if (parsing_options) {
					if (arg == "--") {
						parsing_options = false;
						continue;
					}

					if (arg [0] == '-') {
						if (UnixParseOption (arg, ref args, ref i))
							continue;

						// Try a -CSCOPTION
						string csc_opt = "/" + arg.Substring (1);
						if (CSCParseOption (csc_opt, ref args))
							continue;

						Error_WrongOption (arg);
						return false;
					}
					if (arg [0] == '/') {
						if (CSCParseOption (arg, ref args))
							continue;

						// Need to skip `/home/test.cs' however /test.cs is considered as error
						if (arg.Length < 2 || arg.IndexOf ('/', 2) == -1) {
							Error_WrongOption (arg);
							return false;
						}
					}
				}

				ProcessSourceFiles (arg, false);
			}

			if (require_files == false)
				return true;
					
			//
			// If we are an exe, require a source file for the entry point
			//
			if (RootContext.Target == Target.Exe || RootContext.Target == Target.WinExe || RootContext.Target == Target.Module) {
				if (first_source == null) {
					Report.Error (2008, "No files to compile were specified");
					return false;
				}

			}

			//
			// If there is nothing to put in the assembly, and we are not a library
			//
			if (first_source == null && embedded_resources == null) {
				Report.Error (2008, "No files to compile were specified");
				return false;
			}

			return true;
		}

		public void Parse ()
		{
			Location.Initialize ();

			var cu = Location.SourceFiles;
			for (int i = 0; i < cu.Count; ++i) {
				if (tokenize) {
					tokenize_file (cu [i], ctx);
				} else {
					Parse (cu [i]);
				}
			}
		}

		void ProcessSourceFiles (string spec, bool recurse)
		{
			string path, pattern;

			SplitPathAndPattern (spec, out path, out pattern);
			if (pattern.IndexOf ('*') == -1){
				AddSourceFile (spec);
				return;
			}

			string [] files = null;
			try {
				files = Directory.GetFiles (path, pattern);
			} catch (System.IO.DirectoryNotFoundException) {
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return;
			} catch (System.IO.IOException){
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return;
			}
			foreach (string f in files) {
				AddSourceFile (f);
			}

			if (!recurse)
				return;
			
			string [] dirs = null;

			try {
				dirs = Directory.GetDirectories (path);
			} catch {
			}
			
			foreach (string d in dirs) {
					
				// Don't include path in this string, as each
				// directory entry already does
				ProcessSourceFiles (d + "/" + pattern, true);
			}
		}

		public void ProcessDefaultConfig ()
		{
			if (!load_default_config)
				return;
	
			//
			// For now the "default config" is harcoded into the compiler
			// we can move this outside later
			//
			string [] default_config = {
				"System",
				"System.Xml",
#if NET_2_1
				"System.Net",
				"System.Windows",
				"System.Windows.Browser",
#endif
#if false
				//
				// Is it worth pre-loading all this stuff?
				//
				"Accessibility",
				"System.Configuration.Install",
				"System.Data",
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

			soft_references.AddRange (default_config);

			if (RootContext.Version > LanguageVersion.ISO_2)
				soft_references.Add ("System.Core");
			if (RootContext.Version > LanguageVersion.V_3)
				soft_references.Add ("Microsoft.CSharp");
		}

		public static string OutputFile
		{
			set {
				output_file = value;
			}
			get {
				return Path.GetFileName (output_file);
			}
		}

		void SetWarningLevel (string s)
		{
			int level = -1;

			try {
				level = Int32.Parse (s);
			} catch {
			}
			if (level < 0 || level > 4){
				Report.Error (1900, "Warning level must be in the range 0-4");
				return;
			}
			Report.WarningLevel = level;
		}

		static void Version ()
		{
			string version = Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
			Console.WriteLine ("Mono C# compiler version {0}", version);
			Environment.Exit (0);
		}
		
		//
		// Currently handles the Unix-like command line options, but will be
		// deprecated in favor of the CSCParseOption, which will also handle the
		// options that start with a dash in the future.
		//
		bool UnixParseOption (string arg, ref string [] args, ref int i)
		{
			switch (arg){
			case "-v":
				CSharpParser.yacc_verbose_flag++;
				return true;

			case "--version":
				Version ();
				return true;
				
			case "--parse":
				parse_only = true;
				return true;
				
			case "--main": case "-m":
				Report.Warning (-29, 1, "Compatibility: Use -main:CLASS instead of --main CLASS or -m CLASS");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				RootContext.MainClass = args [++i];
				return true;
				
			case "--unsafe":
				Report.Warning (-29, 1, "Compatibility: Use -unsafe instead of --unsafe");
				RootContext.Unsafe = true;
				return true;
				
			case "/?": case "/h": case "/help":
			case "--help":
				Usage ();
				Environment.Exit (0);
				return true;

			case "--define":
				Report.Warning (-29, 1, "Compatibility: Use -d:SYMBOL instead of --define SYMBOL");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				RootContext.AddConditional (args [++i]);
				return true;

			case "--tokenize": 
				tokenize = true;
				return true;
				
			case "-o": 
			case "--output":
				Report.Warning (-29, 1, "Compatibility: Use -out:FILE instead of --output FILE or -o FILE");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				OutputFile = args [++i];
				return true;

			case "--checked":
				Report.Warning (-29, 1, "Compatibility: Use -checked instead of --checked");
				RootContext.Checked = true;
				return true;
				
			case "--stacktrace":
				Report.Printer.Stacktrace = true;
				return true;
				
			case "--linkresource":
			case "--linkres":
				Report.Warning (-29, 1, "Compatibility: Use -linkres:VALUE instead of --linkres VALUE");
				if ((i + 1) >= args.Length){
					Usage ();
					Report.Error (5, "Missing argument to --linkres"); 
					Environment.Exit (1);
				}
				if (embedded_resources == null)
					embedded_resources = new Resources (ctx);
				
				embedded_resources.Add (false, args [++i], args [i]);
				return true;
				
			case "--resource":
			case "--res":
				Report.Warning (-29, 1, "Compatibility: Use -res:VALUE instead of --res VALUE");
				if ((i + 1) >= args.Length){
					Usage ();
					Report.Error (5, "Missing argument to --resource"); 
					Environment.Exit (1);
				}
				if (embedded_resources == null)
					embedded_resources = new Resources (ctx);
				
				embedded_resources.Add (true, args [++i], args [i]);
				return true;
				
			case "--target":
				Report.Warning (-29, 1, "Compatibility: Use -target:KIND instead of --target KIND");
				if ((i + 1) >= args.Length){
					Environment.Exit (1);
					return true;
				}
				
				string type = args [++i];
				switch (type){
				case "library":
					RootContext.Target = Target.Library;
					RootContext.TargetExt = ".dll";
					break;
					
				case "exe":
					RootContext.Target = Target.Exe;
					break;
					
				case "winexe":
					RootContext.Target = Target.WinExe;
					break;
					
				case "module":
					RootContext.Target = Target.Module;
					RootContext.TargetExt = ".dll";
					break;
				default:
					TargetUsage ();
					break;
				}
				return true;
				
			case "-r":
				Report.Warning (-29, 1, "Compatibility: Use -r:LIBRARY instead of -r library");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				
				string val = args [++i];
				int idx = val.IndexOf ('=');
				if (idx > -1) {
					string alias = val.Substring (0, idx);
					string assembly = val.Substring (idx + 1);
					AddExternAlias (alias, assembly);
					return true;
				}

				references.Add (val);
				return true;
				
			case "-L":
				Report.Warning (-29, 1, "Compatibility: Use -lib:ARG instead of --L arg");
				if ((i + 1) >= args.Length){
					Usage ();	
					Environment.Exit (1);
				}
				link_paths.Add (args [++i]);
				return true;

			case "--lint":
				RootContext.EnhancedWarnings = true;
				return true;
				
			case "--nostdlib":
				Report.Warning (-29, 1, "Compatibility: Use -nostdlib instead of --nostdlib");
				RootContext.StdLib = false;
				return true;
				
			case "--nowarn":
				Report.Warning (-29, 1, "Compatibility: Use -nowarn instead of --nowarn");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				int warn = 0;
				
				try {
					warn = Int32.Parse (args [++i]);
				} catch {
					Usage ();
					Environment.Exit (1);
				}
				Report.SetIgnoreWarning (warn);
				return true;
				
			case "--wlevel":
				Report.Warning (-29, 1, "Compatibility: Use -warn:LEVEL instead of --wlevel LEVEL");
				if ((i + 1) >= args.Length){
					Report.Error (
						1900,
						"--wlevel requires a value from 0 to 4");
					Environment.Exit (1);
				}

				SetWarningLevel (args [++i]);
				return true;

			case "--mcs-debug":
				if ((i + 1) >= args.Length){
					Report.Error (5, "--mcs-debug requires an argument");
					Environment.Exit (1);
				}

				try {
					Report.DebugFlags = Int32.Parse (args [++i]);
				} catch {
					Report.Error (5, "Invalid argument to --mcs-debug");
					Environment.Exit (1);
				}
				return true;
				
			case "--about":
				About ();
				return true;
				
			case "--recurse":
				Report.Warning (-29, 1, "Compatibility: Use -recurse:PATTERN option instead --recurse PATTERN");
				if ((i + 1) >= args.Length){
					Report.Error (5, "--recurse requires an argument");
					Environment.Exit (1);
				}
				ProcessSourceFiles (args [++i], true); 
				return true;
				
			case "--timestamp":
				timestamps = true;
				last_time = first_time = DateTime.Now;
				return true;

			case "--debug": case "-g":
				Report.Warning (-29, 1, "Compatibility: Use -debug option instead of -g or --debug");
				want_debugging_support = true;
				return true;
				
			case "--noconfig":
				Report.Warning (-29, 1, "Compatibility: Use -noconfig option instead of --noconfig");
				load_default_config = false;
				return true;

			default:
				if (arg.StartsWith ("--fatal")){
					if (arg.StartsWith ("--fatal=")){
						if (!Int32.TryParse (arg.Substring (8), out fatal_errors))
							fatal_errors = 1;
					} else
						fatal_errors = 1;
					return true;
				}
				break;
			}

			return false;
		}

#if !SMCS_SOURCE
		public static string GetPackageFlags (string packages, bool fatal, Report report)
		{
			ProcessStartInfo pi = new ProcessStartInfo ();
			pi.FileName = "pkg-config";
			pi.RedirectStandardOutput = true;
			pi.UseShellExecute = false;
			pi.Arguments = "--libs " + packages;
			Process p = null;
			try {
				p = Process.Start (pi);
			} catch (Exception e) {
				report.Error (-27, "Couldn't run pkg-config: " + e.Message);
				if (fatal)
					Environment.Exit (1);
				p.Close ();
				return null;
			}
			
			if (p.StandardOutput == null){
				report.Warning (-27, 1, "Specified package did not return any information");
				p.Close ();
				return null;
			}
			string pkgout = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();
			if (p.ExitCode != 0) {
				report.Error (-27, "Error running pkg-config. Check the above output.");
				if (fatal)
					Environment.Exit (1);
				p.Close ();
				return null;
			}
			p.Close ();

			return pkgout;
		}
#endif

		//
		// This parses the -arg and /arg options to the compiler, even if the strings
		// in the following text use "/arg" on the strings.
		//
		bool CSCParseOption (string option, ref string [] args)
		{
			int idx = option.IndexOf (':');
			string arg, value;

			if (idx == -1){
				arg = option;
				value = "";
			} else {
				arg = option.Substring (0, idx);

				value = option.Substring (idx + 1);
			}

			switch (arg.ToLower (CultureInfo.InvariantCulture)){
			case "/nologo":
				return true;

			case "/t":
			case "/target":
				switch (value){
				case "exe":
					RootContext.Target = Target.Exe;
					break;

				case "winexe":
					RootContext.Target = Target.WinExe;
					break;

				case "library":
					RootContext.Target = Target.Library;
					RootContext.TargetExt = ".dll";
					break;

				case "module":
					RootContext.Target = Target.Module;
					RootContext.TargetExt = ".netmodule";
					break;

				default:
					TargetUsage ();
					break;
				}
				return true;

			case "/out":
				if (value.Length == 0){
					Usage ();
					Environment.Exit (1);
				}
				OutputFile = value;
				return true;

			case "/o":
			case "/o+":
			case "/optimize":
			case "/optimize+":
				RootContext.Optimize = true;
				return true;

			case "/o-":
			case "/optimize-":
				RootContext.Optimize = false;
				return true;

			// TODO: Not supported by csc 3.5+
			case "/incremental":
			case "/incremental+":
			case "/incremental-":
				// nothing.
				return true;

			case "/d":
			case "/define": {
				if (value.Length == 0){
					Usage ();
					Environment.Exit (1);
				}

				foreach (string d in value.Split (argument_value_separator)) {
					string conditional = d.Trim ();
					if (!Tokenizer.IsValidIdentifier (conditional)) {
						Report.Warning (2029, 1, "Invalid conditional define symbol `{0}'", conditional);
						continue;
					}
					RootContext.AddConditional (conditional);
				}
				return true;
			}

			case "/bugreport":
				//
				// We should collect data, runtime, etc and store in the file specified
				//
				Console.WriteLine ("To file bug reports, please visit: http://www.mono-project.com/Bugs");
				return true;
#if !SMCS_SOURCE
			case "/pkg": {
				string packages;

				if (value.Length == 0){
					Usage ();
					Environment.Exit (1);
				}
				packages = String.Join (" ", value.Split (new Char [] { ';', ',', '\n', '\r'}));
				string pkgout = GetPackageFlags (packages, true, Report);
				
				if (pkgout != null){
					string [] xargs = pkgout.Trim (new Char [] {' ', '\n', '\r', '\t'}).
						Split (new Char [] { ' ', '\t'});
					args = AddArgs (args, xargs);
				}
				
				return true;
			}
#endif
			case "/linkres":
			case "/linkresource":
			case "/res":
			case "/resource":
				if (embedded_resources == null)
					embedded_resources = new Resources (ctx);

				bool embeded = arg [1] == 'r' || arg [1] == 'R';
				string[] s = value.Split (argument_value_separator);
				switch (s.Length) {
				case 1:
					if (s[0].Length == 0)
						goto default;
					embedded_resources.Add (embeded, s [0], Path.GetFileName (s[0]));
					break;
				case 2:
					embedded_resources.Add (embeded, s [0], s [1]);
					break;
				case 3:
					if (s [2] != "public" && s [2] != "private") {
						Report.Error (1906, "Invalid resource visibility option `{0}'. Use either `public' or `private' instead", s [2]);
						return true;
					}
					embedded_resources.Add (embeded, s [0], s [1], s [2] == "private");
					break;
				default:
					Report.Error (-2005, "Wrong number of arguments for option `{0}'", option);
					break;
				}

				return true;
				
			case "/recurse":
				if (value.Length == 0){
					Report.Error (5, "-recurse requires an argument");
					Environment.Exit (1);
				}
				ProcessSourceFiles (value, true); 
				return true;

			case "/r":
			case "/reference": {
				if (value.Length == 0){
					Report.Error (5, "-reference requires an argument");
					Environment.Exit (1);
				}

				string[] refs = value.Split (argument_value_separator);
				foreach (string r in refs){
					string val = r;
					int index = val.IndexOf ('=');
					if (index > -1) {
						string alias = r.Substring (0, index);
						string assembly = r.Substring (index + 1);
						AddExternAlias (alias, assembly);
						return true;
					}

					if (val.Length != 0)
						references.Add (val);
				}
				return true;
			}
			case "/addmodule": {
				if (value.Length == 0){
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}

				string[] refs = value.Split (argument_value_separator);
				foreach (string r in refs){
					modules.Add (r);
				}
				return true;
			}
			case "/win32res": {
				if (value.Length == 0) {
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				
				if (win32IconFile != null)
					Report.Error (1565, "Cannot specify the `win32res' and the `win32ico' compiler option at the same time");

				win32ResourceFile = value;
				return true;
			}
			case "/win32icon": {
				if (value.Length == 0) {
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}

				if (win32ResourceFile != null)
					Report.Error (1565, "Cannot specify the `win32res' and the `win32ico' compiler option at the same time");

				win32IconFile = value;
				return true;
			}
			case "/doc": {
				if (value.Length == 0){
					Report.Error (2006, arg + " requires an argument");
					Environment.Exit (1);
				}
				RootContext.Documentation = new Documentation (value);
				return true;
			}
			case "/lib": {
				string [] libdirs;
				
				if (value.Length == 0){
					Report.Error (5, "/lib requires an argument");
					Environment.Exit (1);
				}

				libdirs = value.Split (argument_value_separator);
				foreach (string dir in libdirs)
					link_paths.Add (dir);
				return true;
			}

			case "/debug-":
				want_debugging_support = false;
				return true;
				
			case "/debug":
				if (value == "full" || value == "")
					want_debugging_support = true;

				return true;
				
			case "/debug+":
				want_debugging_support = true;
				return true;

			case "/checked":
			case "/checked+":
				RootContext.Checked = true;
				return true;

			case "/checked-":
				RootContext.Checked = false;
				return true;

			case "/clscheck":
			case "/clscheck+":
				return true;

			case "/clscheck-":
				RootContext.VerifyClsCompliance = false;
				return true;

			case "/unsafe":
			case "/unsafe+":
				RootContext.Unsafe = true;
				return true;

			case "/unsafe-":
				RootContext.Unsafe = false;
				return true;

			case "/warnaserror":
			case "/warnaserror+":
				if (value.Length == 0) {
					Report.WarningsAreErrors = true;
				} else {
					foreach (string wid in value.Split (argument_value_separator))
						Report.AddWarningAsError (wid);
				}
				return true;

			case "/warnaserror-":
				if (value.Length == 0) {
					Report.WarningsAreErrors = false;
				} else {
					foreach (string wid in value.Split (argument_value_separator))
						Report.RemoveWarningAsError (wid);
				}
				return true;

			case "/warn":
				SetWarningLevel (value);
				return true;

			case "/nowarn": {
				string [] warns;

				if (value.Length == 0){
					Report.Error (5, "/nowarn requires an argument");
					Environment.Exit (1);
				}

				warns = value.Split (argument_value_separator);
				foreach (string wc in warns){
					try {
						if (wc.Trim ().Length == 0)
							continue;

						int warn = Int32.Parse (wc);
						if (warn < 1) {
							throw new ArgumentOutOfRangeException("warn");
						}
						Report.SetIgnoreWarning (warn);
					} catch {
						Report.Error (1904, String.Format("`{0}' is not a valid warning number", wc));
					}
				}
				return true;
			}

			case "/noconfig":
				load_default_config = false;
				return true;

			case "/platform":
				switch (value.ToLower (CultureInfo.InvariantCulture)) {
				case "anycpu":
					RootContext.Platform = Platform.AnyCPU;
					break;
				case "x86":
					RootContext.Platform = Platform.X86;
					break;
				case "x64":
					RootContext.Platform = Platform.X64;
					break;
				case "itanium":
					RootContext.Platform = Platform.IA64;
					break;
				default:
					Report.Error (1672, "Invalid platform type for -platform. Valid options are `anycpu', `x86', `x64' or `itanium'");
					break;
				}

				return true;

				// We just ignore this.
			case "/errorreport":
			case "/filealign":
				return true;
				
			case "/help2":
				OtherFlags ();
				Environment.Exit(0);
				return true;
				
			case "/help":
			case "/?":
				Usage ();
				Environment.Exit (0);
				return true;

			case "/main":
			case "/m":
				if (value.Length == 0){
					Report.Error (5, arg + " requires an argument");					
					Environment.Exit (1);
				}
				RootContext.MainClass = value;
				return true;

			case "/nostdlib":
			case "/nostdlib+":
				RootContext.StdLib = false;
				return true;

			case "/nostdlib-":
				RootContext.StdLib = true;
				return true;

			case "/fullpaths":
				return true;

			case "/keyfile":
				if (value == String.Empty) {
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				RootContext.StrongNameKeyFile = value;
				return true;
			case "/keycontainer":
				if (value == String.Empty) {
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				RootContext.StrongNameKeyContainer = value;
				return true;
			case "/delaysign+":
				RootContext.StrongNameDelaySign = true;
				return true;
			case "/delaysign-":
				RootContext.StrongNameDelaySign = false;
				return true;

			case "/langversion":
				switch (value.ToLower (CultureInfo.InvariantCulture)) {
				case "iso-1":
					RootContext.Version = LanguageVersion.ISO_1;
					return true;	
				case "default":
					RootContext.Version = LanguageVersion.Default;
					RootContext.AddConditional ("__V2__");
					return true;
				case "iso-2":
					RootContext.Version = LanguageVersion.ISO_2;
					return true;
				case "3":
					RootContext.Version = LanguageVersion.V_3;
					return true;
				case "future":
					RootContext.Version = LanguageVersion.Future;
					return true;
				}

				Report.Error (1617, "Invalid -langversion option `{0}'. It must be `ISO-1', `ISO-2', `3' or `Default'", value);
				return true;

			case "/codepage":
				switch (value) {
				case "utf8":
					encoding = new UTF8Encoding();
					break;
				case "reset":
					encoding = Encoding.Default;
					break;
				default:
					try {
						encoding = Encoding.GetEncoding (
						Int32.Parse (value));
					} catch {
						Report.Error (2016, "Code page `{0}' is invalid or not installed", value);
					}
					break;
				}
				return true;
			}

			return false;
		}

		void Error_WrongOption (string option)
		{
			Report.Error (2007, "Unrecognized command-line option: `{0}'", option);
		}

		static string [] AddArgs (string [] args, string [] extra_args)
		{
			string [] new_args;
			new_args = new string [extra_args.Length + args.Length];

			// if args contains '--' we have to take that into account
			// split args into first half and second half based on '--'
			// and add the extra_args before --
			int split_position = Array.IndexOf (args, "--");
			if (split_position != -1)
			{
				Array.Copy (args, new_args, split_position);
				extra_args.CopyTo (new_args, split_position);
				Array.Copy (args, split_position, new_args, split_position + extra_args.Length, args.Length - split_position);
			}
			else
			{
				args.CopyTo (new_args, 0);
				extra_args.CopyTo (new_args, args.Length);
			}

			return new_args;
		}

		void AddExternAlias (string identifier, string assembly)
		{
			if (assembly.Length == 0) {
				Report.Error (1680, "Invalid reference alias '" + identifier + "='. Missing filename");
				return;
			}

			if (!IsExternAliasValid (identifier)) {
				Report.Error (1679, "Invalid extern alias for /reference. Alias '" + identifier + "' is not a valid identifier");
				return;
			}
			
			// Could here hashtable throw an exception?
			external_aliases [identifier] = assembly;
		}
		
		static bool IsExternAliasValid (string identifier)
		{
			if (identifier.Length == 0)
				return false;
			if (identifier [0] != '_' && !Char.IsLetter (identifier [0]))
				return false;

			for (int i = 1; i < identifier.Length; i++) {
				char c = identifier [i];
				if (Char.IsLetter (c) || Char.IsDigit (c))
					continue;

				UnicodeCategory category = Char.GetUnicodeCategory (c);
				if (category != UnicodeCategory.Format || category != UnicodeCategory.NonSpacingMark ||
						category != UnicodeCategory.SpacingCombiningMark ||
						category != UnicodeCategory.ConnectorPunctuation)
					return false;
			}
			
			return true;
		}

		//
		// Main compilation method
		//
		public bool Compile ()
		{
			// TODO: Should be passed to parser as an argument
			RootContext.ToplevelTypes = new ModuleCompiled (ctx, RootContext.Unsafe);
			var ctypes = TypeManager.InitCoreTypes ();

			Parse ();
			if (Report.Errors > 0)
				return false;

			if (tokenize || parse_only)
				return true;

			if (RootContext.ToplevelTypes.NamespaceEntry != null)
				throw new InternalErrorException ("who set it?");

			ProcessDefaultConfig ();

			//
			// Quick hack
			//
			if (output_file == null){
				if (first_source == null){
					Report.Error (1562, "If no source files are specified you must specify the output file with -out:");
					return false;
				}
					
				int pos = first_source.LastIndexOf ('.');

				if (pos > 0)
					output_file = first_source.Substring (0, pos) + RootContext.TargetExt;
				else
					output_file = first_source + RootContext.TargetExt;
			}

			if (!CodeGen.Init (output_file, output_file, want_debugging_support, ctx))
				return false;

			if (RootContext.Target == Target.Module) {
				PropertyInfo module_only = typeof (AssemblyBuilder).GetProperty ("IsModuleOnly", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if (module_only == null) {
					Report.RuntimeMissingSupport (Location.Null, "/target:module");
					Environment.Exit (1);
				}

				MethodInfo set_method = module_only.GetSetMethod (true);
				set_method.Invoke (CodeGen.Assembly.Builder, BindingFlags.Default, null, new object[]{true}, null);
			}

			ctx.GlobalRootNamespace.AddModuleReference (RootContext.ToplevelTypes.Builder);

			//
			// Load assemblies required
			//
			if (timestamps)
				ShowTime ("Loading references");

			ctx.MetaImporter.Initialize ();
			LoadReferences ();		
		
			if (timestamps)
				ShowTime ("References loaded");
			
			if (!TypeManager.InitCoreTypes (ctx, ctypes))
				return false;

			TypeManager.InitOptionalCoreTypes (ctx);

			if (timestamps)
				ShowTime ("   Core Types done");

			//
			// The second pass of the compiler
			//
			if (timestamps)
				ShowTime ("Resolving tree");
			RootContext.ResolveTree ();

			if (Report.Errors > 0)
				return false;
			if (timestamps)
				ShowTime ("Populate tree");

			RootContext.PopulateTypes ();

			if (Report.Errors == 0 &&
				RootContext.Documentation != null &&
				!RootContext.Documentation.OutputDocComment (
					output_file, Report))
				return false;

			//
			// Verify using aliases now
			//
			NamespaceEntry.VerifyAllUsing ();
			
			if (Report.Errors > 0){
				return false;
			}

			CodeGen.Assembly.Resolve ();
			
			if (RootContext.VerifyClsCompliance) {
				if (CodeGen.Assembly.IsClsCompliant) {
					AttributeTester.VerifyModulesClsCompliance (ctx);
				}
			}
			if (Report.Errors > 0)
				return false;
			
			//
			// The code generator
			//
			if (timestamps)
				ShowTime ("Emitting code");
			ShowTotalTime ("Total so far");
			RootContext.EmitCode ();
			if (timestamps)
				ShowTime ("   done");

			if (Report.Errors > 0){
				return false;
			}

			if (timestamps)
				ShowTime ("Closing types");

			RootContext.CloseTypes (ctx);

			PEFileKinds k = PEFileKinds.ConsoleApplication;

			switch (RootContext.Target) {
			case Target.Library:
			case Target.Module:
				k = PEFileKinds.Dll; break;
			case Target.Exe:
				k = PEFileKinds.ConsoleApplication; break;
			case Target.WinExe:
				k = PEFileKinds.WindowApplication; break;
			}

			if (RootContext.NeedsEntryPoint) {
				Method ep = RootContext.EntryPoint;

				if (ep == null) {
					if (RootContext.MainClass != null) {
						DeclSpace main_cont = RootContext.ToplevelTypes.GetDefinition (RootContext.MainClass) as DeclSpace;
						if (main_cont == null) {
							Report.Error (1555, "Could not find `{0}' specified for Main method", RootContext.MainClass); 
							return false;
						}

						if (!(main_cont is ClassOrStruct)) {
							Report.Error (1556, "`{0}' specified for Main method must be a valid class or struct", RootContext.MainClass);
							return false;
						}

						Report.Error (1558, main_cont.Location, "`{0}' does not have a suitable static Main method", main_cont.GetSignatureForError ());
						return false;
					}

					if (Report.Errors == 0)
						Report.Error (5001, "Program `{0}' does not contain a static `Main' method suitable for an entry point",
							output_file);
					return false;
				}

				CodeGen.Assembly.Builder.SetEntryPoint (ep.MethodBuilder, k);
			} else if (RootContext.MainClass != null) {
				Report.Error (2017, "Cannot specify -main if building a module or library");
			}

			if (embedded_resources != null){
				if (RootContext.Target == Target.Module) {
					Report.Error (1507, "Cannot link resource file when building a module");
					return false;
				}

				embedded_resources.Emit ();
			}

			//
			// Add Win32 resources
			//

			if (win32ResourceFile != null) {
				try {
					CodeGen.Assembly.Builder.DefineUnmanagedResource (win32ResourceFile);
				} catch (ArgumentException) {
					Report.RuntimeMissingSupport (Location.Null, "resource embedding ");
				}
			} else {
				CodeGen.Assembly.Builder.DefineVersionInfoResource ();
			}

			if (win32IconFile != null) {
				MethodInfo define_icon = typeof (AssemblyBuilder).GetMethod ("DefineIconResource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (define_icon == null) {
					Report.RuntimeMissingSupport (Location.Null, "resource embedding");
				} else {
					define_icon.Invoke (CodeGen.Assembly.Builder, new object [] { win32IconFile });
				}
			}

			if (Report.Errors > 0)
				return false;
			
			CodeGen.Save (output_file, want_debugging_support, Report);
			if (timestamps) {
				ShowTime ("Saved output");
				ShowTotalTime ("Total");
			}

			Timer.ShowTimers ();

#if DEBUGME
			Console.WriteLine ("Size of strings held: " + DeclSpace.length);
			Console.WriteLine ("Size of strings short: " + DeclSpace.small);
#endif
			return (Report.Errors == 0);
		}
	}

	class Resources
	{
		interface IResource
		{
			void Emit (CompilerContext cc);
			string FileName { get; }
		}

		class EmbededResource : IResource
		{
			static MethodInfo embed_res;
			readonly object[] args;

			public EmbededResource (string name, string file, bool isPrivate)
			{
				args = new object [3];
				args [0] = name;
				args [1] = file;
				args [2] = isPrivate ? ResourceAttributes.Private : ResourceAttributes.Public;
			}

			public void Emit (CompilerContext cc)
			{
				if (embed_res == null) {
					var argst = new [] {
						typeof (string), typeof (string), typeof (ResourceAttributes)
					};

					embed_res = typeof (AssemblyBuilder).GetMethod (
						"EmbedResourceFile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
						null, CallingConventions.Any, argst, null);

					if (embed_res == null) {
						cc.Report.RuntimeMissingSupport (Location.Null, "Resource embedding");
					}
				}

				embed_res.Invoke (CodeGen.Assembly.Builder, args);
			}

			public string FileName {
				get {
					return (string)args [1];
				}
			}
		}

		class LinkedResource : IResource
		{
			readonly string file;
			readonly string name;
			readonly ResourceAttributes attribute;

			public LinkedResource (string name, string file, bool isPrivate)
			{
				this.name = name;
				this.file = file;
				this.attribute = isPrivate ? ResourceAttributes.Private : ResourceAttributes.Public;
			}

			public void Emit (CompilerContext cc)
			{
				CodeGen.Assembly.Builder.AddResourceFile (name, Path.GetFileName(file), attribute);
			}

			public string FileName {
				get {
					return file;
				}
			}
		}


		Dictionary<string, IResource> embedded_resources = new Dictionary<string, IResource> ();
		readonly CompilerContext ctx;

		public Resources (CompilerContext ctx)
		{
			this.ctx = ctx;
		}

		public void Add (bool embeded, string file, string name)
		{
			Add (embeded, file, name, false);
		}

		public void Add (bool embeded, string file, string name, bool isPrivate)
		{
			if (embedded_resources.ContainsKey (name)) {
				ctx.Report.Error (1508, "The resource identifier `{0}' has already been used in this assembly", name);
				return;
			}
			IResource r = embeded ? 
				(IResource) new EmbededResource (name, file, isPrivate) : 
				new LinkedResource (name, file, isPrivate);

			embedded_resources.Add (name, r);
		}

		public void Emit ()
		{
			foreach (IResource r in embedded_resources.Values) {
				if (!File.Exists (r.FileName)) {
					ctx.Report.Error (1566, "Error reading resource file `{0}'", r.FileName);
					continue;
				}
				
				r.Emit (ctx);
			}
		}
	}

	//
	// This is the only public entry point
	//
	public class CompilerCallableEntryPoint : MarshalByRefObject {
		public static bool InvokeCompiler (string [] args, TextWriter error)
		{
			try {
				StreamReportPrinter srp = new StreamReportPrinter (error);
				Driver d = Driver.Create (args, true, srp);
				if (d == null)
					return false;

				return d.Compile () && srp.ErrorsCount == 0;
			} finally {
				Reset ();
			}
		}

		public static int[] AllWarningNumbers {
			get {
				return Report.AllWarnings;
			}
		}

		public static void Reset ()
		{
			Reset (true);
		}

		public static void PartialReset ()
		{
			Reset (false);
		}
		
		public static void Reset (bool full_flag)
		{
			Driver.Reset ();
			CSharpParser.yacc_verbose_flag = 0;
			Location.Reset ();

			if (!full_flag)
				return;

			RootContext.Reset (full_flag);
			TypeManager.Reset ();
			ArrayContainer.Reset ();
			ReferenceContainer.Reset ();
			PointerContainer.Reset ();
			Parameter.Reset ();

			Unary.Reset ();
			UnaryMutator.Reset ();
			Binary.Reset ();
			ConstantFold.Reset ();
			CastFromDecimal.Reset ();
			StringConcat.Reset ();
			
			NamespaceEntry.Reset ();
			CodeGen.Reset ();
			Attribute.Reset ();
			AnonymousTypeClass.Reset ();
			AnonymousMethodBody.Reset ();
			AnonymousMethodStorey.Reset ();
			SymbolWriter.Reset ();
			Switch.Reset ();
			Linq.QueryBlock.TransparentParameter.Reset ();
			Convert.Reset ();
			TypeInfo.Reset ();
		}
	}
}
