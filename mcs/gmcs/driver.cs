//
// driver.cs: The compiler command line driver.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
//

namespace Mono.CSharp
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;
	using System.IO;
	using System.Text;
	using System.Globalization;

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
		// mscorlib.dll here.
		static ArrayList references;

		//
		// If any of these fail, we ignore the problem.  This is so
		// that we can list all the assemblies in Windows and not fail
		// if they are missing on Linux.
		//
		static ArrayList soft_references;

		// Lookup paths
		static ArrayList link_paths;

		// Whether we want Yacc to output its progress
		static bool yacc_verbose = false;

		// Whether we want to only run the tokenizer
		static bool tokenize = false;
		
		static string first_source;

		static Target target = Target.Exe;
		static string target_ext = ".exe";

		static bool want_debugging_support = false;

		static bool parse_only = false;
		static bool timestamps = false;
		static bool pause = false;
		static bool show_counters = false;
		
		//
		// Whether to load the initial config file (what CSC.RSP has by default)
		// 
		static bool load_default_config = true;

		static Hashtable response_file_list;

		//
		// A list of resource files
		//
		static ArrayList resources;
		static ArrayList embedded_resources;
		
		//
		// An array of the defines from the command line
		//
		static ArrayList defines;

		//
		// Output file
		//
		static string output_file = null;

		//
		// Last time we took the time
		//
		static DateTime last_time, first_time;

		//
		// Encoding: ISO-Latin1 is 28591
		//
		static Encoding encoding;

		//
		// Whether the user has specified a different encoder manually
		//
		static bool using_default_encoder = true;
		
		public static void ShowTime (string msg)
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

		public static void ShowTotalTime (string msg)
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
	       
		static void tokenize_file (SourceFile file)
		{
			Stream input;

			try {
				input = File.OpenRead (file.Name);
			} catch {
				Report.Error (2001, "Source file '" + file.Name + "' could not be opened");
				return;
			}

			using (input){
				StreamReader reader = new StreamReader (input, encoding, using_default_encoder);
				Tokenizer lexer = new Tokenizer (reader, file, defines);
				int token, tokens = 0, errors = 0;

				while ((token = lexer.token ()) != Token.EOF){
					Location l = lexer.Location;
					tokens++;
					if (token == Token.ERROR)
						errors++;
				}
				Console.WriteLine ("Tokenized: " + tokens + " found " + errors + " errors");
			}
			
			return;
		}

		// MonoTODO("Change error code for aborted compilation to something reasonable")]		
		static void parse (SourceFile file)
		{
			CSharpParser parser;
			Stream input;

			try {
				input = File.OpenRead (file.Name);
			} catch {
				Report.Error (2001, "Source file '" + file.Name + "' could not be opened");
				return;
			}

			StreamReader reader = new StreamReader (input, encoding, using_default_encoder);
				
			parser = new CSharpParser (reader, file, defines);
			parser.yacc_verbose_flag = yacc_verbose;
			try {
				parser.parse ();
			} catch (Exception ex) {
				Report.Error(666, "Compilation aborted: " + ex);
			} finally {
				input.Close ();
			}
		}
		
		static void Usage ()
		{
			Console.WriteLine (
				"Mono C# compiler, (C) 2001 - 2003 Ximian, Inc.\n" +
				"mcs [options] source-files\n" +
				"   --about            About the Mono C# compiler\n" +
				"   -checked[+|-]      Set default context to checked\n" +
				"   -codepage:ID       Sets code page to the one in ID\n" +
				"                      (number, `utf8' or `reset')\n" +
				"   -define:S1[;S2]    Defines one or more symbols (short: /d:)\n" +
				"   -debug[+-]         Generate debugging information\n" + 
				"   -doc:FILE          XML Documentation file to generate\n" + 
				"   -g                 Generate debugging information\n" +
				"   --fatal            Makes errors fatal\n" +
				"   -lib:PATH1,PATH2   Adds the paths to the assembly link path\n" +
				"   -main:class        Specified the class that contains the entry point\n" +
				"   -noconfig[+|-]     Disables implicit references to assemblies\n" +
				"   -nostdlib[+|-]     Does not load core libraries\n" +
				"   -nowarn:W1[,W2]    Disables one or more warnings\n" + 
				"   -out:FNAME         Specifies output file\n" +
				"   --parse            Only parses the source file\n" +
				"   --expect-error X   Expect that error X will be encountered\n" +
				"   -recurse:SPEC      Recursively compiles the files in SPEC ([dir]/file)\n" + 
				"   -reference:ASS     References the specified assembly (-r:ASS)\n" +
				"   --stacktrace       Shows stack trace at error location\n" +
				"   -target:KIND       Specifies the target (KIND is one of: exe, winexe,\n" +
				"                      library, module), (short: /t:)\n" +
				"   --timestamp        Displays time stamps of various compiler events\n" +
				"   -unsafe[+|-]       Allows unsafe code\n" +
				"   -warnaserror[+|-]  Treat warnings as errors\n" +
				"   -warn:LEVEL        Sets warning level (the highest is 4, the default)\n" +
				"   -v                 Verbose parsing (for debugging the parser)\n" +
				"\n" +
				"Resources:\n" +
				"   -linkresource:FILE[,ID] Links FILE as a resource\n" +
				"   -resource:FILE[,ID]     Embed FILE as a resource\n" +
				"   --mcs-debug X      Sets MCS debugging level to X\n" +
                                "   @file              Read response file for more options\n\n" +
				"Options can be of the form -option or /option");
		}

		static void TargetUsage ()
		{
			Report.Error (2019, "Valid options for -target: are exe, winexe, library or module");
		}
		
		static void About ()
		{
			Console.WriteLine (
				"The Mono C# compiler is (C) 2001, 2002, 2003 Ximian, Inc.\n\n" +
				"The compiler source code is released under the terms of the GNU GPL\n\n" +

				"For more information on Mono, visit the project Web site\n" +
				"   http://www.go-mono.com\n\n" +

				"The compiler was written by Miguel de Icaza, Ravi Pratap and Martin Baulig");
			Environment.Exit (0);
		}

		public static int counter1, counter2;
		
		public static int Main (string[] args)
		{
			RootContext.V2 = true;
			bool ok = MainDriver (args);
			
			if (ok && Report.Errors == 0) {
				Console.Write("Compilation succeeded");
				if (Report.Warnings > 0) {
					Console.Write(" - {0} warning(s)", Report.Warnings);
				}
				Console.WriteLine();
				if (show_counters){
					Console.WriteLine ("Counter1: " + counter1);
					Console.WriteLine ("Counter2: " + counter2);
				}
				if (pause)
					Console.ReadLine ();
				return 0;
			} else {
				Console.WriteLine("Compilation failed: {0} error(s), {1} warnings",
					Report.Errors, Report.Warnings);
				return 1;
			}
		}

		static public void LoadAssembly (string assembly, bool soft)
		{
			Assembly a;
			string total_log = "";

			try {
				char[] path_chars = { '/', '\\', '.' };

				if (assembly.IndexOfAny (path_chars) != -1) {
					a = Assembly.LoadFrom (assembly);
				} else {
					a = Assembly.Load (assembly);
				}
				TypeManager.AddAssembly (a);

			} catch (FileNotFoundException){
				foreach (string dir in link_paths){
					string full_path = Path.Combine (dir, assembly);
					if (!assembly.EndsWith (".dll"))
						full_path += ".dll";

					try {
						a = Assembly.LoadFrom (full_path);
						TypeManager.AddAssembly (a);
						return;
					} catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				if (!soft) {
					Report.Error (6, "Cannot find assembly `" + assembly + "'" );
					Console.WriteLine ("Log: \n" + total_log);
				}
			} catch (BadImageFormatException f) {
				Report.Error(6, "Cannot load assembly (bad file format)" + f.FusionLog);
			} catch (FileLoadException f){
				Report.Error(6, "Cannot load assembly " + f.FusionLog);
			} catch (ArgumentNullException){
				Report.Error(6, "Cannot load assembly (null argument)");
			}
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		static public void LoadReferences ()
		{
			foreach (string r in references)
				LoadAssembly (r, false);

			foreach (string r in soft_references)
				LoadAssembly (r, true);
			
			return;
		}

		static void SetupDefaultDefines ()
		{
			defines = new ArrayList ();
			defines.Add ("__MonoCS__");
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
				string codebase = a.Location;
				if (codebase.EndsWith ("corlib.dll")){
					return codebase.Substring (0, codebase.LastIndexOf (System.IO.Path.DirectorySeparatorChar));
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

		static void ProcessFile (string f)
		{
			if (first_source == null)
				first_source = f;

			Location.AddFile (f);
		}

		static void ProcessFiles ()
		{
			Location.Initialize ();

			foreach (SourceFile file in Location.SourceFiles) {
				if (tokenize) {
					tokenize_file (file);
				} else {
					parse (file);
				}
			}
		}

		static void CompileFiles (string spec, bool recurse)
		{
			string path, pattern;

			SplitPathAndPattern (spec, out path, out pattern);
			if (pattern.IndexOf ("*") == -1){
				ProcessFile (spec);
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
				ProcessFile (f);
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
				CompileFiles (d + "/" + pattern, true);
			}
		}

		static void DefineDefaultConfig ()
		{
			//
			// For now the "default config" is harcoded into the compiler
			// we can move this outside later
			//
			string [] default_config = {
				"System",
				"System.Xml",
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
			
			int p = 0;
			foreach (string def in default_config)
				soft_references.Insert (p++, def);
		}

		static void SetOutputFile (string name)
		{
			output_file = name;
		}

		static void SetWarningLevel (string s)
		{
			int level = 0;

			try {
				level = Int32.Parse (s);
			} catch {
				Report.Error (
					1900,
					"--wlevel requires an value from 0 to 4");
				Environment.Exit (1);
			}
			if (level < 0 || level > 4){
				Report.Error (1900, "Warning level must be 0 to 4");
				Environment.Exit (1);
			} else
				RootContext.WarningLevel = level;
		}

		static void SetupV2 ()
		{
			RootContext.V2 = true;
			defines.Add ("__V2__");
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
		static bool UnixParseOption (string arg, ref string [] args, ref int i)
		{
			switch (arg){
			case "-v":
				yacc_verbose = true;
				return true;

			case "--version":
				Version ();
				return true;
				
			case "--parse":
				parse_only = true;
				return true;
				
			case "--main": case "-m":
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				RootContext.MainClass = args [++i];
				return true;
				
			case "--unsafe":
				RootContext.Unsafe = true;
				return true;
				
			case "/?": case "/h": case "/help":
			case "--help":
				Usage ();
				Environment.Exit (0);
				return true;
				
			case "--define":
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				defines.Add (args [++i]);
				return true;

			case "--show-counters":
				show_counters = true;
				return true;
				
			case "--expect-error": {
				int code = 0;
				
				try {
					code = Int32.Parse (
						args [++i], NumberStyles.AllowLeadingSign);
					Report.ExpectedError = code;
				} catch {
					Report.Error (-14, "Invalid number specified");
				} 
				return true;
			}
				
			case "--tokenize": 
				tokenize = true;
				return true;
				
			case "-o": 
			case "--output":
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				SetOutputFile (args [++i]);
				return true;
				
			case "--checked":
				RootContext.Checked = true;
				return true;
				
			case "--stacktrace":
				Report.Stacktrace = true;
				return true;
				
			case "--linkresource":
			case "--linkres":
				if ((i + 1) >= args.Length){
					Usage ();
					Report.Error (5, "Missing argument to --linkres"); 
					Environment.Exit (1);
				}
				if (resources == null)
					resources = new ArrayList ();
				
				resources.Add (args [++i]);
				return true;
				
			case "--resource":
			case "--res":
				if ((i + 1) >= args.Length){
					Usage ();
					Report.Error (5, "Missing argument to --resource"); 
					Environment.Exit (1);
				}
				if (embedded_resources == null)
					embedded_resources = new ArrayList ();
				
				embedded_resources.Add (args [++i]);
				return true;
				
			case "--target":
				if ((i + 1) >= args.Length){
					Environment.Exit (1);
					return true;
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
					TargetUsage ();
					Environment.Exit (1);
					break;
				}
				return true;
				
			case "-r":
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				
				references.Add (args [++i]);
				return true;
				
			case "-L":
				if ((i + 1) >= args.Length){
					Usage ();	
					Environment.Exit (1);
				}
				link_paths.Add (args [++i]);
				return true;
				
			case "--nostdlib":
				RootContext.StdLib = false;
				return true;
				
			case "--fatal":
				Report.Fatal = true;
				return true;
				
			case "--werror":
				Report.WarningsAreErrors = true;
				return true;
				
			case "--nowarn":
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
				if ((i + 1) >= args.Length){
					Report.Error (
						1900,
						"--wlevel requires an value from 0 to 4");
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
				if ((i + 1) >= args.Length){
					Report.Error (5, "--recurse requires an argument");
					Environment.Exit (1);
				}
				CompileFiles (args [++i], true); 
				return true;
				
			case "--timestamp":
				timestamps = true;
				last_time = first_time = DateTime.Now;
				return true;

			case "--pause":
				pause = true;
				return true;
				
			case "--debug": case "-g":
				want_debugging_support = true;
				return true;
				
			case "--noconfig":
				load_default_config = false;
				return true;
			}

			return false;
		}

		//
		// Currently it is very basic option parsing, but eventually, this will
		// be the complete option parser
		//
		static bool CSCParseOption (string option, ref string [] args, ref int i)
		{
			int idx = option.IndexOf (":");
			string arg, value;

			if (idx == -1){
				arg = option;
				value = "";
			} else {
				arg = option.Substring (0, idx);

				value = option.Substring (idx + 1);
			}

			switch (arg){
			case "/nologo":
				return true;

			case "/t":
			case "/target":
				switch (value){
				case "exe":
					target = Target.Exe;
					break;

				case "winexe":
					target = Target.WinExe;
					break;

				case "library":
					target = Target.Library;
					target_ext = ".dll";
					break;

				case "module":
					target = Target.Module;
					target_ext = ".netmodule";
					break;

				default:
					TargetUsage ();
					Environment.Exit (1);
					break;
				}
				return true;

			case "/out":
				if (value == ""){
					Usage ();
					Environment.Exit (1);
				}
				SetOutputFile (value);
				return true;

			case "/optimize":
			case "/optimize+":
			case "/optimize-":
			case "/incremental":
			case "/incremental+":
			case "/incremental-":
				// nothing.
				return true;

			case "/d":
			case "/define": {
				string [] defs;

				if (value == ""){
					Usage ();
					Environment.Exit (1);
				}

				defs = value.Split (new Char [] {';', ','});
				foreach (string d in defs){
					defines.Add (d);
				}
				return true;
			}

			case "/linkres":
			case "/linkresource":
				if (value == ""){
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				if (resources == null)
					resources = new ArrayList ();
				
				resources.Add (value);
				return true;
				
			case "/res":
			case "/resource":
				if (value == ""){
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				if (embedded_resources == null)
					embedded_resources = new ArrayList ();
				
				embedded_resources.Add (value);
				return true;
				
			case "/recurse":
				if (value == ""){
					Report.Error (5, "/recurse requires an argument");
					Environment.Exit (1);
				}
				CompileFiles (value, true); 
				return true;

			case "/r":
			case "/reference": {
				if (value == ""){
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}

				string [] refs = value.Split (new char [] { ';', ',' });
				foreach (string r in refs){
					references.Add (r);
				}
				return true;
			}
			case "/doc": {
				if (value == ""){
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				// TODO handle the /doc argument to generate xml doc
				return true;
			}
			case "/lib": {
				string [] libdirs;
				
				if (value == ""){
					Report.Error (5, "/lib requires an argument");
					Environment.Exit (1);
				}

				libdirs = value.Split (new Char [] { ',' });
				foreach (string dir in libdirs)
					link_paths.Add (dir);
				return true;
			}
				
			case "/debug":
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

			case "/unsafe":
			case "/unsafe+":
				RootContext.Unsafe = true;
				return true;

			case "/unsafe-":
				RootContext.Unsafe = false;
				return true;

			case "/warnaserror":
			case "/warnaserror+":
				Report.WarningsAreErrors = true;
				return true;

			case "/warnaserror-":
				Report.WarningsAreErrors = false;
				return true;

			case "/warn":
				SetWarningLevel (value);
				return true;

			case "/nowarn": {
				string [] warns;

				if (value == ""){
					Report.Error (5, "/nowarn requires an argument");
					Environment.Exit (1);
				}
				
				warns = value.Split (new Char [] {','});
				foreach (string wc in warns){
					int warn = 0;
					
					try {
						warn = Int32.Parse (wc);
					} catch {
						Usage ();
						Environment.Exit (1);
					}
					Report.SetIgnoreWarning (warn);
				}
				return true;
			}

			case "/noconfig-":
				load_default_config = true;
				return true;
				
			case "/noconfig":
			case "/noconfig+":
				load_default_config = false;
				return true;

			case "/help":
			case "/?":
				Usage ();
				Environment.Exit (0);
				return true;

			case "/main":
			case "/m":
				if (value == ""){
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

			case "/win32icon":
				Report.Error (5, "/win32icon is currently not supported");
				return true;
				
			case "/v2":
				SetupV2 ();
				return true;
				
			case "/codepage":
				int cp = -1;

				if (value == "utf8"){
					encoding = new UTF8Encoding();
					using_default_encoder = false;
					return true;
				}
				if (value == "reset"){
					//
					// 28591 is the code page for ISO-8859-1 encoding.
					//
					cp = 28591;
					using_default_encoder = true;
				}
				
				try {
					cp = Int32.Parse (value);
				} catch { }
				
				if (cp == -1){
					Console.WriteLine ("Invalid code-page requested");
					Usage ();
				}

				try {
					encoding = Encoding.GetEncoding (cp);
					using_default_encoder = false;
				} catch {
					Console.WriteLine ("Code page: {0} not supported", cp);
				}
				return true;

			}
			return false;
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
		// [MonoTODO("Change error code for unknown argument to something reasonable")]
		static bool MainDriver (string [] args)
		{
			int i;
			bool parsing_options = true;

			Console.WriteLine ("Mono C# Compiler {0} for Generics",
					   Assembly.GetExecutingAssembly ().GetName ().Version.ToString ());
			try {
				encoding = Encoding.GetEncoding (28591);
			} catch {
				Console.WriteLine ("Error: could not load encoding 28591, trying 1252");
				encoding = Encoding.GetEncoding (1252);
			}
			
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
						return false;
					}

					new_args = new string [extra_args.Length + argc];
					args.CopyTo (new_args, 0);
					extra_args.CopyTo (new_args, argc);
					args = new_args;
					argc = new_args.Length;
					continue;
				}

				if (parsing_options){
					if (arg == "--"){
						parsing_options = false;
						continue;
					}
					
					if (arg.StartsWith ("-")){
						if (UnixParseOption (arg, ref args, ref i))
							continue;

						// Try a -CSCOPTION
						string csc_opt = "/" + arg.Substring (1);
						if (CSCParseOption (csc_opt, ref args, ref i))
							continue;
					} else {
						if (arg.StartsWith ("/")){
							if (CSCParseOption (arg, ref args, ref i))
								continue;
						}
					}
				}

				CompileFiles (arg, false); 
			}

			ProcessFiles ();

			if (tokenize)
				return true;
			
			if (first_source == null){
				Report.Error (2008, "No files to compile were specified");
				return false;
			}

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

			if (Report.Errors > 0){
				return false;
			}

			//
			// Load assemblies required
			//
			if (timestamps)
				ShowTime ("Loading references");
			link_paths.Add (GetSystemDir ());
			LoadReferences ();
			
			if (timestamps)
				ShowTime ("   References loaded");
			
			if (Report.Errors > 0){
				return false;
			}

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

			CodeGen.Init (output_file, output_file, want_debugging_support);

			TypeManager.AddModule (CodeGen.ModuleBuilder);

			DateTime start = DateTime.Now;
			TypeManager.ComputeNamespaces ();
			DateTime end = DateTime.Now;
			
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
			if (!RootContext.StdLib)
				RootContext.BootCorlib_PopulateCoreTypes ();
			RootContext.PopulateTypes ();
			RootContext.DefineTypes ();
			
			TypeManager.InitCodeHelpers ();

			//
			// Verify using aliases now
			//
			Namespace.VerifyUsing ();
			
			if (Report.Errors > 0){
				return false;
			}
			
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

			RootContext.CloseTypes ();

			PEFileKinds k = PEFileKinds.ConsoleApplication;
				
			if (target == Target.Library || target == Target.Module){
				k = PEFileKinds.Dll;

				if (RootContext.MainClass != null)
					Report.Error (2017, "Can not specify -main: when building module or library");
			} else if (target == Target.Exe)
				k = PEFileKinds.ConsoleApplication;
			else if (target == Target.WinExe)
				k = PEFileKinds.WindowApplication;

			if (target == Target.Exe || target == Target.WinExe){
				MethodInfo ep = RootContext.EntryPoint;

				if (ep == null){
					if (Report.Errors == 0)
						Report.Error (5001, "Program " + output_file +
							      " does not have an entry point defined");
					return false;
				}
				
				CodeGen.AssemblyBuilder.SetEntryPoint (ep, k);
			}

			//
			// Add the resources
			//
			if (resources != null){
				foreach (string spec in resources){
					string file, res;
					int cp;
					
					cp = spec.IndexOf (',');
					if (cp != -1){
						file = spec.Substring (0, cp);
						res = spec.Substring (cp + 1);
					} else
						file = res = spec;

					CodeGen.AssemblyBuilder.AddResourceFile (res, file);
				}
			}
			
			if (embedded_resources != null){
				object[] margs = new object [2];
				Type[] argst = new Type [2];
				argst [0] = argst [1] = typeof (string);
				MethodInfo embed_res = typeof (AssemblyBuilder).GetMethod ("EmbedResourceFile", argst);
				if (embed_res == null) {
					Report.Warning (0, new Location (-1), "Cannot embed resources on this runtime: try the Mono runtime instead.");
				} else {
					foreach (string spec in embedded_resources) {
						int cp;

						cp = spec.IndexOf (',');
						if (cp != -1){
							margs [0] = spec.Substring (cp + 1);
							margs [1] = spec.Substring (0, cp);
						} else
							margs [0] = margs [1] = spec;

						if (File.Exists ((string) margs [1]))
							embed_res.Invoke (CodeGen.AssemblyBuilder, margs);
						else {
							Report.Error (1566, "Can not find the resource " + margs [1]);
						}
					}
				}
			}

			if (Report.Errors > 0)
				return false;
			
			CodeGen.Save (output_file);
			if (timestamps) {
				ShowTime ("Saved output");
				ShowTotalTime ("Total");
			}

			Timer.ShowTimers ();
			
			if (Report.ExpectedError != 0){
				Console.WriteLine("Failed to report expected error " + Report.ExpectedError);
				Environment.Exit (1);
				return false;
			}

#if DEBUGME
			Console.WriteLine ("Size of strings held: " + DeclSpace.length);
			Console.WriteLine ("Size of strings short: " + DeclSpace.small);
#endif
			return (Report.Errors == 0);
		}

	}
}
