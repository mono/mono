using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

public enum Target {
	Library, Exe, Module, WinExe
}

public enum LanguageVersion
{
	ISO_1		= 1,
	Default_MCS	= 2,
	ISO_2		= 3,
	LINQ		= 4,
	Future		= 5,
	Default		= LINQ
}

class MsbuildGenerator {
	static string read ()
	{
		return Console.ReadLine ();
	}

	// Currently used
	static bool Unsafe = false;
	static StringBuilder defines = new StringBuilder ();
	static bool StdLib = true;

	// Currently unused
	static Target Target = Target.Exe;
	static string TargetExt = ".exe";
	static string OutputFile;
	static bool Optimize = true;
	static bool VerifyClsCompliance = true;

	static string win32IconFile;
	static bool want_debugging_support = true;
	static bool Checked = false;
	static bool WarningsAreErrors;
	static Dictionary<string,string> embedded_resources = new Dictionary<string,string> ();
	static List<string> references = new List<string> ();
	static List<string> warning_as_error = new List<string> ();
	static int WarningLevel = 4;
	static List<int> ignore_warning = new List<int> ();
	static bool load_default_config = true;
	static string StrongNameKeyFile;
	static string StrongNameKeyContainer;
	static bool StrongNameDelaySign = false;
	static LanguageVersion Version = LanguageVersion.Default;
	static string CodePage;

	static readonly char[] argument_value_separator = new char [] { ';', ',' };

	static void Usage ()
	{
		Console.WriteLine ("Invalid argument");
	}
	
	//
	// This parses the -arg and /arg options to the compiler, even if the strings
	// in the following text use "/arg" on the strings.
	//
	static bool CSCParseOption (string option, ref string [] args)
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
				Target = Target.Exe;
				break;

			case "winexe":
				Target = Target.WinExe;
				break;

			case "library":
				Target = Target.Library;
				TargetExt = ".dll";
				break;

			case "module":
				Target = Target.Module;
				TargetExt = ".netmodule";
				break;

			default:
				return false;
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
			Optimize = true;
			return true;

		case "/o-":
		case "/optimize-":
			Optimize = false;
			return true;

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

			foreach (string d in value.Split (argument_value_separator)){
				if (defines.Length != 0)
					defines.Append (";");
				defines.Append (d);
			}

			return true;
		}

		case "/bugreport":
			//
			// We should collect data, runtime, etc and store in the file specified
			//
			return true;
		case "/linkres":
		case "/linkresource":
		case "/res":
		case "/resource":
			bool embeded = arg [1] == 'r' || arg [1] == 'R';
			string[] s = value.Split (argument_value_separator);
			switch (s.Length) {
			case 1:
				if (s[0].Length == 0)
					goto default;
				embedded_resources [s[0]] = Path.GetFileName (s[0]);
				break;
			case 2:
				embedded_resources [s [0]] = s [1];
				break;
			case 3:
				Console.WriteLine ("Does not support this method yet: {0}", arg);
				Environment.Exit (1);
				break;
			default:
				Console.WriteLine ("Wrong number of arguments for option `{0}'", option);
				Environment.Exit (1);
				break;
				
			}

			return true;
				
		case "/recurse":
			Console.WriteLine ("/recurse not supported");
			Environment.Exit (1);
			return true;

		case "/r":
		case "/reference": {
			if (value.Length == 0){
				Console.WriteLine ("-reference requires an argument");
				Environment.Exit (1);
			}

			string[] refs = value.Split (argument_value_separator);
			foreach (string r in refs){
				string val = r;
				int index = val.IndexOf ('=');
				if (index > -1) {
					Console.WriteLine ("/reference = not supported");
					Environment.Exit (1);
					//string alias = r.Substring (0, index);
					//string assembly = r.Substring (index + 1);
					//AddExternAlias (alias, assembly);
					//return true;
				}

				if (val.Length != 0)
					references.Add (val);
			}
			return true;
		}
		case "/main":
		case "/m":
		case "/addmodule": 
		case "/win32res":
		case "/doc": 
		case "/lib": 
		{
			Console.WriteLine ("{0} = not supported", option);
			Environment.Exit (1);
			return true;
		}
		case "/win32icon": {
			win32IconFile = value;
			return true;
		}
		case "/debug-":
			want_debugging_support = false;
			return true;
				
		case "/debug":
		case "/debug+":
			want_debugging_support = true;
			return true;

		case "/checked":
		case "/checked+":
			Checked = true;
			return true;

		case "/checked-":
			Checked = false;
			return true;

		case "/clscheck":
		case "/clscheck+":
			return true;

		case "/clscheck-":
			VerifyClsCompliance = false;
			return true;

		case "/unsafe":
		case "/unsafe+":
			Unsafe = true;
			return true;

		case "/unsafe-":
			Unsafe = false;
			return true;

		case "/warnaserror":
		case "/warnaserror+":
			if (value.Length == 0) {
				WarningsAreErrors = true;
			} else {
				foreach (string wid in value.Split (argument_value_separator))
					warning_as_error.Add (wid);
			}
			return true;

		case "/warnaserror-":
			if (value.Length == 0) {
				WarningsAreErrors = false;
			} else {
				foreach (string wid in value.Split (argument_value_separator))
					warning_as_error.Remove (wid);
			}
			return true;

		case "/warn":
			WarningLevel = Int32.Parse (value);
			return true;

		case "/nowarn": {
			string [] warns;

			if (value.Length == 0){
				Console.WriteLine ("/nowarn requires an argument");
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
					ignore_warning.Add (warn);
				} catch {
					Console.WriteLine (String.Format("`{0}' is not a valid warning number", wc));
					Environment.Exit (1);
				}
			}
			return true;
		}

		case "/noconfig":
			load_default_config = false;
			return true;

		case "/nostdlib":
		case "/nostdlib+":
			StdLib = false;
			return true;

		case "/nostdlib-":
			StdLib = true;
			return true;

		case "/fullpaths":
			return true;

		case "/keyfile":
			if (value == String.Empty) {
				Console.WriteLine ("{0} requires an argument", arg);
				Environment.Exit (1);
			}
			StrongNameKeyFile = value;
			return true;
		case "/keycontainer":
			if (value == String.Empty) {
				Console.WriteLine ("{0} requires an argument", arg);
				Environment.Exit (1);
			}
			StrongNameKeyContainer = value;
			return true;
		case "/delaysign+":
			StrongNameDelaySign = true;
			return true;
		case "/delaysign-":
			StrongNameDelaySign = false;
			return true;

		case "/langversion":
			switch (value.ToLower (CultureInfo.InvariantCulture)) {
			case "iso-1":
				Version = LanguageVersion.ISO_1;
				return true;
					
			case "default":
				Version = LanguageVersion.Default;
				return true;
			case "iso-2":
				Version = LanguageVersion.ISO_2;
				return true;
			case "future":
				Version = LanguageVersion.Future;
				return true;
			}
			Console.WriteLine ("Invalid option `{0}' for /langversion. It must be either `ISO-1', `ISO-2' or `Default'", value);
			Environment.Exit (1);
			return true;
			
		case "/codepage":
			CodePage = value;
			return true;
		}

		return false;
	}
	
	static void Main (string [] args)
	{
		if (args.Length != 1){
			Console.WriteLine ("You must specify the template file");
			return;
		}
		
		string boot, mcs, flags, output_name, built_sources, library_output, response;

		boot = read ();
		mcs = read ();
		flags = read ();
		output_name = read ();
		built_sources = read ();
		library_output = read ();
		response = read ();

		string [] f = flags.Split ();
		for (int i = 0; i < f.Length; i++){
			if (f [i][0] == '/')
				f [i] = "/" + f [i].Substring (1);

			if (CSCParseOption (f [0], ref f))
				continue;
			Console.WriteLine ("Failure");
			Environment.Exit (1);
		}

		string [] source_files;
		using (var reader = new StreamReader (response)){
			source_files  = reader.ReadToEnd ().Split ();
		}
		StringBuilder sources = new StringBuilder ();
		foreach (string s in source_files){
			if (s.Length == 0)
				continue;
			sources.Append (String.Format ("   <Compile Include=\"{0}\" />\n", s));
		}
		
		var input = new StreamReader (args [0]);
		string template = input.ReadToEnd ();

		//
		// Replace the template values
		//

		//
		// ARGH: The CscToolPath will require more temporary setups to work properly
		// corlib does this:
		// MONO_PATH=./../../class/lib/net_2_0: false ./../../mcs/gmcs.exe
		//
		// this means:
		//    runtime:  ../../class/lib/net_2_0
		//    compiler: ../../mcs/gmcs.exe
		//
		// And our current `csc' script fails for this as it assumes that the
		// compiler will be in the same place as the runtime
		//
		string output = template.
			Replace ("@DEFINES@", defines.ToString ()).
			Replace ("@NOSTDLIB@", StdLib ? "" : "<NoStdLib>true</NoStdLib>").
			Replace ("@ALLOWUNSAFE@", Unsafe ? "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>" : "").
			Replace ("@ASSEMBLYNAME@", Path.GetFileNameWithoutExtension (output_name)).
			Replace ("@OUTPUTDIR@", Path.GetDirectoryName (library_output)).
			Replace ("@SOURCES@", sources.ToString ());

		Console.WriteLine (output);
	}
}