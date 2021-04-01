//
// mkbundle: tool to create bundles.
//
// Based on the `make-bundle' Perl script written by Paolo Molaro (lupus@debian.org)
//
// TODO:
//   [x] Rename the paths for the zip file that is downloaded
//   [x] Update documentation with new flag
//   [x] Load internationalized assemblies
//   [x] Dependencies - if System.dll -> include Mono.Security.* (not needed, automatic)
//   [x] --list-targets should download from a different url
//   [x] --fetch-target should unpack zip file
//   [x] Update --cross to use not a runtime, but an SDK
//   [x] Update --local-targets to show the downloaded SDKs
//
// Author:
//   Miguel de Icaza
//
// (C) Novell, Inc 2004
// (C) 2016 Xamarin Inc
//
// Missing features:
// * Add support for packaging native libraries, extracting at runtime and setting the library path.
// * Implement --list-targets lists all the available remote targets
//
using System;
using System.Diagnostics;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using IKVM.Reflection;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

class MakeBundle {
	static string output = "a.out";
	static string object_out = null;
	static List<string> link_paths = new List<string> ();
	static List<string> aot_paths = new List<string> ();
	static List<string> aot_names = new List<string> ();
	static Dictionary<string,string> libraries = new Dictionary<string,string> ();
	static bool autodeps = false;
	static string in_tree = null;
	static bool keeptemp = false;
	static bool compile_only = false;
	static bool static_link = false;

	// Points to the $sysconfig/mono/4.5/machine.config, which contains System.Configuration settings
	static string machine_config_file = null;

        // By default, we automatically bundle a machine-config, use this to turn off the behavior.
        static bool no_machine_config = false;

	// Points to the $sysconfig/mono/config file, contains <dllmap> and others
	static string config_file = null;

        // By default, we automatically bundle the above config file, use this to turn off the behavior.
	static bool no_config = false;
	
	static string config_dir = null;
	static string style = "linux";
	static bool bundled_header = false;
	static string os_message = "";
	static bool compress;
	static bool nomain;
	static string custom_main = null;
	static bool? use_dos2unix = null;
	static bool skip_scan;
	static string ctor_func;
	static bool quiet = true;
	static string cross_target = null;
	static string fetch_target = null;
	static bool custom_mode = true;
	static string embedded_options = null;
	
	static string runtime = null;

	static bool aot_compile = false;
	static string aot_args = "static";
	static DirectoryInfo aot_temp_dir = null;
	static string aot_mode = "";
	static string aot_runtime = null;
	static string aot_dedup_assembly = null;
	static string cil_strip_path = null;
	static string managed_linker_path = null;
	static string sdk_path = null;
	static string lib_path = null;
	static Dictionary<string,string> environment = new Dictionary<string,string>();
	static string [] i18n = new string [] {
		"West",
		""
	};
	static string [] i18n_all = new string [] {
		"CJK", 
		"MidEast",
		"Other",
		"Rare",
		"West",
		""
	};
	static string target_server = "https://download.mono-project.com/runtimes/raw/";
	static string mono_api_struct_file;

	static int Main (string [] args)
	{
		List<string> sources = new List<string> ();
		int top = args.Length;
		link_paths.Add (".");

		DetectOS ();

		for (int i = 0; i < top; i++){
			switch (args [i]){
			case "--help": case "-h": case "-?":
				Help ();
				return 1;

			case "--simple":
				custom_mode = false;
				autodeps = true;
				break;

			case "-v":
				quiet = false;
				break;
				
			case "--i18n":
				if (i+1 == top){
					Help ();
					return 1;
				}
				var iarg = args [++i];
				if (iarg == "all")
					i18n = i18n_all;
				else if (iarg == "none")
					i18n = new string [0];
				else
					i18n = iarg.Split (',');
				break;
				
			case "--custom":
				custom_mode = true;
				break;
				
			case "-c":
				compile_only = true;
				break;

			case "--local-targets":
				CommandLocalTargets ();
				return 0;

			case "--cross":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				if (sdk_path != null || runtime != null)
					Error ("You can only specify one of --runtime, --sdk or --cross {sdk_path}/{runtime}");
				custom_mode = false;
				autodeps = true;
				cross_target = args [++i];
				break;

			case "--library":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				if (custom_mode){
					Console.Error.WriteLine ("--library can only be used with --simple/--runtime/--cross mode");
					Help ();
					return 1;
				}
				var lspec = args [++i];
				var p = lspec.IndexOf (",");
				string alias, path;
				if (p == -1){
					alias = Path.GetFileName (lspec);
					path = lspec;
				} else {
					alias = lspec.Substring (0, p);
					path = lspec.Substring (p+1);
				}
				if (!File.Exists (path))
					Error ($"The specified library file {path} does not exist");
				libraries [alias] = path;
				break;

			case "--fetch-target":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				fetch_target = args [++i];
				break;

			case "--list-targets":
				CommandLocalTargets ();
				var wc = new WebClient ();
				var s = wc.DownloadString (new Uri (target_server + "target-sdks.txt"));
				Console.WriteLine ("Targets available for download with --fetch-target:\n" + s);
				return 0;
				
			case "--target-server":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				target_server = args [++i];
				break;

			case "-o": 
				if (i+1 == top){
					Help (); 
					return 1;
				}
				output = args [++i];
				break;

			case "--options":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				embedded_options = args [++i];
				break;
			case "--sdk":
				if (i + 1 == top) {
					Help ();
					return 1;
				}
				custom_mode = false;
				autodeps = true;
				sdk_path = args [++i];
				if (cross_target != null || runtime != null)
					Error ("You can only specify one of --runtime, --sdk or --cross");
				break;
			case "--runtime":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				if (sdk_path != null || cross_target != null)
					Error ("You can only specify one of --runtime, --sdk or --cross");
				custom_mode = false;
				autodeps = true;
				runtime = args [++i];
				break;
			case "-oo":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				object_out = args [++i];
				break;

			case "-L":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				link_paths.Add (args [++i]);
				break;

			case "--nodeps":
				autodeps = false;
				break;

			case "--deps":
				autodeps = true;
				break;

			case "--keeptemp":
				keeptemp = true;
				break;
				
			case "--static":
				static_link = true;
				break;
			case "--config":
				if (i+1 == top) {
					Help ();
					return 1;
				}

				config_file = args [++i];
				break;
			case "--machine-config":
				if (i+1 == top) {
					Help ();
					return 1;
				}

				machine_config_file = args [++i];

				if (!quiet)
					Console.WriteLine ("WARNING:\n  Check that the machine.config file you are bundling\n  doesn't contain sensitive information specific to this machine.");
				break;
			case "--no-machine-config":
                                no_machine_config = true;
                                break;
			case "--no-config":
				no_config = true;
				break;
			case "--config-dir":
				if (i+1 == top) {
					Help ();
					return 1;
				}

				config_dir = args [++i];
				break;
			case "-z":
				compress = true;
				break;
			case "--nomain":
				nomain = true;
				break;
			case "--custom-main":
				if (i+1 == top) {
					Help ();
					return 1;
				}
				custom_main = args [++i];
				break;
			case "--style":
				if (i+1 == top) {
					Help ();
					return 1;
				}
				style = args [++i];
				switch (style) {
				case "windows":
				case "mac":
				case "linux":
					break;
				default:
					Error ("Invalid style '{0}' - only 'windows', 'mac' and 'linux' are supported for --style argument", style);
					return 1;
				}
					
				break;
			case "--skip-scan":
				skip_scan = true;
				break;
			case "--static-ctor":
				if (i+1 == top) {
					Help ();
					return 1;
				}
				ctor_func = args [++i];
				break;
			case "--dos2unix":
			case "--dos2unix=true":
				use_dos2unix = true;
				break;
			case "--dos2unix=false":
				use_dos2unix = false;
				break;
			case "-q":
			case "--quiet":
				quiet = true;
				break;
			case "-e":
			case "--env":
				if (i+1 == top) {
					Help ();
					return 1;
				}
				var env = args [++i];
				p = env.IndexOf ('=');
				if (p == -1)
					environment.Add (env, "");
				else
					environment.Add (env.Substring (0, p), env.Substring (p+1));
				break;
			case "--bundled-header":
				bundled_header = true;
				break;
			case "--in-tree":
				if (i+1 == top) {
					Console.WriteLine ("Usage: --in-tree <path/to/headers> ");
					return 1;
				}
				in_tree = args [++i];
				break;
			case "--managed-linker":
				if (i+1 == top) {
					Console.WriteLine ("Usage: --managed-linker <path/to/exe> ");
					return 1;
				}
				managed_linker_path = args [++i];
				break;
			case "--cil-strip":
				if (i+1 == top) {
					Console.WriteLine ("Usage: --cil-strip <path/to/exe> ");
					return 1;
				}
				cil_strip_path = args [++i];
				break;
			case "--aot-runtime":
				if (i+1 == top) {
					Console.WriteLine ("Usage: --aot-runtime <path/to/runtime> ");
					return 1;
				}
				aot_runtime = args [++i];
				aot_compile = true;
				static_link = true;
				break;
			case "--aot-dedup":
				if (i+1 == top) {
					Console.WriteLine ("Usage: --aot-dedup <container_dll> ");
					return 1;
				}
				var rel_path = args [++i];
				var asm = LoadAssembly (rel_path);
				if (asm != null)
					aot_dedup_assembly = new Uri(asm.CodeBase).LocalPath;

				sources.Add (rel_path);
				aot_compile = true;
				static_link = true;
				break;
			case "--aot-mode":
				if (i+1 == top) {
					Console.WriteLine ("Need string of aot mode (full, llvmonly, hybrid). Omit for normal AOT.");
					return 1;
				}

				aot_mode = args [++i];
				if (aot_mode != "full" && aot_mode != "llvmonly" && aot_mode != "hybrid") {
					Console.WriteLine ("Need string of aot mode (full, llvmonly, hybrid). Omit for normal AOT.");
					return 1;
				}

				aot_compile = true;
				static_link = true;
				break;
			case "--aot-args":
				if (i+1 == top) {
					Console.WriteLine ("AOT arguments are passed as a comma-delimited list");
					return 1;
				}
				if (args [i + 1].Contains ("outfile")) {
					Console.WriteLine ("Per-aot-output arguments (ex: outfile, llvm-outfile) cannot be given");
					return 1;
				}
				aot_args = String.Format("static,{0}", args [++i]);
				aot_compile = true;
				static_link = true;
				break;
			case "--mono-api-struct-path":
				if (i+1 == top) {
					Console.WriteLine ("Usage: --mono-api-struct-path <path/to/file>");
					return 1;
				}
				mono_api_struct_file = args [++i];
				break;
			default:
				sources.Add (args [i]);
				break;
			}

		}
		// Modern bundling starts here
		if (!custom_mode){
			if (runtime != null){
				// Nothing to do here, the user has chosen to manually specify --runtime and libraries
			} else if (sdk_path != null) {
				VerifySdk (sdk_path);
			} else if (cross_target == "default" || cross_target == null){
				sdk_path = Path.GetFullPath (Path.Combine (Process.GetCurrentProcess().MainModule.FileName, "..", ".."));
				VerifySdk (sdk_path);
			} else {
				sdk_path = Path.Combine (targets_dir, cross_target);
				Console.WriteLine ("From: " + sdk_path);
				VerifySdk (sdk_path);
			}
		}

		if (fetch_target != null){
			var directory = Path.Combine (targets_dir, fetch_target);
			var zip_download = Path.Combine (directory, "sdk.zip");

			if(Directory.Exists(directory)){
				if(!quiet)
					Console.WriteLine ($"Deleting existing directory: {directory}");
				Directory.Delete(directory, true);
			}

			Directory.CreateDirectory (directory);
			var wc = new WebClient ();
			var uri = new Uri ($"{target_server}{fetch_target}");
			try {
				if (!quiet){
					Console.WriteLine ($"Downloading runtime {uri} to {zip_download}");
				}
				
				wc.DownloadFile (uri, zip_download);
				ZipFile.ExtractToDirectory(zip_download, directory);
				File.Delete (zip_download);
			} catch {
				Console.Error.WriteLine ($"Failure to download the specified runtime from {uri}");
				File.Delete (zip_download);
				return 1;
			}
			return 0;
		}
		
		if (!quiet) {
			Console.WriteLine (os_message);
			Console.WriteLine ("Sources: {0} Auto-dependencies: {1}", sources.Count, autodeps);
		}

		if (sources.Count == 0 || output == null) {
			Help ();
			Environment.Exit (1);
		}

		List<string> assemblies = LoadAssemblies (sources);
		LoadLocalizedAssemblies (assemblies);
		List<string> files = new List<string> ();
		foreach (string file in assemblies)
			if (!QueueAssembly (files, file))
				return 1;

		PreprocessAssemblies (assemblies, files);

		if (aot_compile)
			AotCompile (files);

		if (custom_mode)
			GenerateBundles (files);
		else 
			GeneratePackage (files);

		Console.WriteLine ("Generated {0}", output);

		return 0;
	}

	static void VerifySdk (string path)
	{
		if (!Directory.Exists (path))
			Error ($"The specified SDK path does not exist: {path}");
		runtime = Path.Combine (sdk_path, "bin", "mono");
		if (!File.Exists (runtime)){
			if (File.Exists (runtime + ".exe"))
				runtime += ".exe";
			else
				Error ($"The SDK location does not contain a {path}/bin/mono runtime");
		}
		lib_path = Path.Combine (path, "lib", "mono", "4.5");
		if (!Directory.Exists (lib_path))
			Error ($"The SDK location does not contain a {path}/lib/mono/4.5 directory");
		link_paths.Add (lib_path);
                if (machine_config_file == null && !no_machine_config) {
                        machine_config_file = Path.Combine (path, "etc", "mono", "4.5", "machine.config");
                        if (!File.Exists (machine_config_file)){
                                Error ($"Could not locate the file machine.config file at ${machine_config_file} use --machine-config FILE or --no-machine-config");
                        }
                }
                if (config_file == null && !no_config) {
                        config_file = Path.Combine (path, "etc", "mono", "config");
                        if (!File.Exists (config_file)){
                                Error ($"Could not locate the file config file at ${config_file} use --config FILE or --no-config");
                        }
                }
	}

	static string targets_dir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".mono", "targets");
	
	static void CommandLocalTargets ()
	{
		string [] targets;

		Console.WriteLine ("Available targets locally:");
		Console.WriteLine ("\tdefault\t- Current System Mono");
		try {
			targets = Directory.GetDirectories (targets_dir);
		} catch {
			return;
		}
		foreach (var target in targets){
			var p = Path.Combine (target, "bin", "mono");
			if (File.Exists (p))
				Console.WriteLine ("\t{0}", Path.GetFileName (target));
		}
	}

	static void WriteSymbol (StreamWriter sw, string name, long size)
	{
		switch (style){
		case "linux":
			sw.WriteLine (
				".globl {0}\n" +
				"\t.section .rodata\n" +
				"\t.p2align 5\n" +
				"\t.type {0}, \"object\"\n" +
				"\t.size {0}, {1}\n" +
				"{0}:\n",
				name, size);
			break;
		case "osx":
			sw.WriteLine (
				"\t.section __TEXT,__text,regular,pure_instructions\n" + 
				"\t.globl _{0}\n" +
				"\t.data\n" +
				"\t.align 4\n" +
				"_{0}:\n",
				name, size);
			break;
		case "windows":
			string mangled_symbol_name = "";
			if (Target64BitApplication())
				mangled_symbol_name = name;
			else
				mangled_symbol_name = "_" + name;

			sw.WriteLine (
				".globl {0}\n" +
				"\t.section .rdata,\"dr\"\n" +
				"\t.align 32\n" +
				"{0}:\n",
				mangled_symbol_name);
			break;
		}
	}
	
	static string [] chars = new string [256];
	
	static void WriteBuffer (StreamWriter ts, Stream stream, byte[] buffer)
	{
		int n;
		
		// Preallocate the strings we need.
		if (chars [0] == null) {
			for (int i = 0; i < chars.Length; i++)
				chars [i] = string.Format ("{0}", i.ToString ());
		}

		while ((n = stream.Read (buffer, 0, buffer.Length)) != 0) {
			int count = 0;
			for (int i = 0; i < n; i++) {
				if (count % 32 == 0) {
					ts.Write ("\n\t.byte ");
				} else {
					ts.Write (",");
				}
				ts.Write (chars [buffer [i]]);
				count ++;
			}
		}

		ts.WriteLine ();
	}

	class PackageMaker {
		Dictionary<string, Tuple<long,int>> locations = new Dictionary<string, Tuple<long,int>> ();
		int align = 4096; // first non-Windows alignment, saving on average 30K
		Stream package;
		
		public PackageMaker (string runtime, string output)
		{
			package = File.Create (output, 128*1024);
			if (IsUnix){
				File.SetAttributes (output, unchecked ((FileAttributes) 0x80000000));
			}

			Console.WriteLine ("Using runtime: " + runtime);

			// Probe for MZ signature to decide if we are targeting Windows,
			// so we can optimize an average of 30K away on Unix.
			using (Stream runtimeStream = File.OpenRead (runtime)) {
				var runtimeBuffer = new byte [2];
				if (runtimeStream.Read (runtimeBuffer, 0, 2) == 2
					&& runtimeBuffer [0] == (byte)'M'
					&& runtimeBuffer [1] == (byte)'Z')
					align = 1 << 16; // first Windows alignment
			}
			AddFile (runtime);
		}

		public int AddFile (string fname)
		{
			using (Stream fileStream = File.OpenRead (fname)){
				var ret = fileStream.Length;

				if (!quiet)
					Console.WriteLine ("At {0:x} with input {1}", package.Position, fileStream.Length);
				fileStream.CopyTo (package);
				package.Position = package.Position + (align - (package.Position % align));
				align = 4096; // rest of alignment for all systems
				return (int) ret;
			}
		}
		
		public void Add (string entry, string fname)
		{
			var p = package.Position;
			var size = AddFile (fname);
			locations [entry] = Tuple.Create(p, size);
		}

		public void AddString (string entry, string text)
		{
			// FIXME Strings are over-aligned?
			var bytes = Encoding.UTF8.GetBytes (text);
			locations [entry] = Tuple.Create (package.Position, bytes.Length);
			package.Write (bytes, 0, bytes.Length);
			package.WriteByte (0);
			package.Position = package.Position + (align - (package.Position % align));
		}

		public void AddStringPair (string entry, string key, string value)
		{
			// FIXME Strings are over-aligned?
			var kbytes = Encoding.UTF8.GetBytes (key);
			var vbytes = Encoding.UTF8.GetBytes (value);

			Console.WriteLine ("ADDING {0} to {1}", key, value);
			if (kbytes.Length > 255){
				Console.WriteLine ("The key value can not exceed 255 characters: " + key);
				Environment.Exit (1);
			}
				
			locations [entry] = Tuple.Create (package.Position, kbytes.Length+vbytes.Length+3);
			package.WriteByte ((byte)kbytes.Length);
			package.Write (kbytes, 0, kbytes.Length);
			package.WriteByte (0);
			package.Write (vbytes, 0, vbytes.Length);
			package.WriteByte (0);
			package.Position = package.Position + (align - (package.Position % align));
		}

		public void Dump ()
		{
			if (quiet)
				return;
			foreach (var floc in locations.Keys){
				Console.WriteLine ($"{floc} at {locations[floc]:x}");
			}
		}

		public void WriteIndex ()
		{
			var indexStart = package.Position;
			var binary = new BinaryWriter (package);

			binary.Write (locations.Count);
			foreach (var entry in from entry in locations orderby entry.Value.Item1 ascending select entry){
				var bytes = Encoding.UTF8.GetBytes (entry.Key);
				binary.Write (bytes.Length+1);
				binary.Write (bytes);
				binary.Write ((byte) 0);
				binary.Write (entry.Value.Item1);
				binary.Write (entry.Value.Item2);
			}
			binary.Write (indexStart);
			binary.Write (Encoding.UTF8.GetBytes ("xmonkeysloveplay"));
			binary.Flush ();
		}
		
		public void Close ()
		{
			WriteIndex ();
			package.Close ();
			package = null;
		}
	}

	static bool MaybeAddFile (PackageMaker maker, string code, string file)
	{
		if (file == null)
			return true;
		
		if (!File.Exists (file)){
			Error ("The file {0} does not exist", file);
			return false;
		}
		maker.Add (code, file);
		// add a space after code (="systemconfig:" or "machineconfig:")
		Console.WriteLine (code + " " + file);
		return true;
	}
	
	static bool GeneratePackage (List<string> files)
	{
		if (runtime == null){
			if (IsUnix)
				runtime = Process.GetCurrentProcess().MainModule.FileName;
			else {
				Error ("You must specify at least one runtime with --runtime or --cross");
				Environment.Exit (1);
			}
		}
		if (!File.Exists (runtime)){
			Error ($"The specified runtime at {runtime} does not exist");
			Environment.Exit (1);
		}
		
		if (ctor_func != null){
			Error ("--static-ctor not supported with package bundling, you must use native compilation for this");
			return false;
		}
		
		var maker = new PackageMaker (runtime, output);
		
		foreach (var url in files){
			string fname = LocateFile (new Uri (url).LocalPath);
			string aname = MakeBundle.GetAssemblyName (fname);

			maker.Add ("assembly:" + aname, fname);
			Console.WriteLine ("     Assembly: " + fname);
			if (File.Exists (fname + ".config")){
				maker.Add ("config:" + aname, fname + ".config");
				Console.WriteLine ("       Config: " + fname + ".config");
			}
		}
		
		if (!MaybeAddFile (maker, "systemconfig:", config_file) || !MaybeAddFile (maker, "machineconfig:", machine_config_file))
			return false;

		if (config_dir != null){
			maker.Add ("config_dir:", config_dir);
			Console.WriteLine ("   Config_dir: " + config_dir );
		}
		if (embedded_options != null)
			maker.AddString ("options:", embedded_options);
		if (environment.Count > 0){
			foreach (var key in environment.Keys)
				maker.AddStringPair ("env:" + key, key, environment [key]);
		}
		if (libraries.Count > 0){
			foreach (var alias_and_path in libraries){
				Console.WriteLine ("     Library:  " + alias_and_path.Value);
				maker.Add ("library:" + alias_and_path.Key, alias_and_path.Value);
			}
		}
		maker.Dump ();
		maker.Close ();
		return true;
	}
	
	static void GenerateBundles (List<string> files)
	{
		string temp_s = "temp.s"; // Path.GetTempFileName ();
		string temp_c = "temp.c";
		string temp_o = (style != "windows") ? "temp.o" : "temp.s.obj";

		if (compile_only)
			temp_c = output;
		if (object_out != null)
			temp_o = object_out;
		
		try {
			List<string> c_bundle_names = new List<string> ();
			List<string[]> config_names = new List<string[]> ();

			using (StreamWriter ts = new StreamWriter (File.Create (temp_s))) {
			using (StreamWriter tc = new StreamWriter (File.Create (temp_c))) {
			string prog = null;

			if (bundled_header) {
				tc.WriteLine ("/* This source code was produced by mkbundle, do not edit */");
				tc.WriteLine ("\n#ifndef NULL\n#define NULL (void *)0\n#endif");
				tc.WriteLine (@"
typedef struct {
	const char *name;
	const unsigned char *data;
	const unsigned int size;
} MonoBundledAssembly;
");

				// These values are part of the public API, so they are expected not to change
				tc.WriteLine("#define MONO_AOT_MODE_NORMAL 1");
				tc.WriteLine("#define MONO_AOT_MODE_FULL 3");
				tc.WriteLine("#define MONO_AOT_MODE_LLVMONLY 4");
			} else {
				tc.WriteLine ("#include <mono/metadata/mono-config.h>");
				tc.WriteLine ("#include <mono/metadata/assembly.h>\n");

				if (in_tree != null)
					tc.WriteLine ("#include <mono/mini/jit.h>\n");
				else
					tc.WriteLine ("#include <mono/jit/jit.h>\n");
			}

			Stream template_stream;
			if (String.IsNullOrEmpty (mono_api_struct_file)) {
				tc.WriteLine ("#define USE_DEFAULT_MONO_API_STRUCT");
				template_stream = typeof (MakeBundle).Assembly.GetManifestResourceStream ("bundle-mono-api.inc");
			} else {
				template_stream = File.OpenRead (mono_api_struct_file);
			}

			StreamReader s;
			using (s = new StreamReader (template_stream)) {
				tc.Write (s.ReadToEnd ());
			}
			template_stream.Dispose ();

			if (compress) {
				tc.WriteLine ("#define USE_COMPRESSED_ASSEMBLY\n");
				tc.WriteLine ("typedef struct _compressed_data {");
				tc.WriteLine ("\tMonoBundledAssembly assembly;");
				tc.WriteLine ("\tint compressed_size;");
				tc.WriteLine ("} CompressedAssembly;\n");
			}

			object monitor = new object ();

			var streams = new Dictionary<string, Stream> ();
			var sizes = new Dictionary<string, long> ();

			// Do the file reading and compression in parallel
			Action<string> body = delegate (string url) {
				string fname = LocateFile (new Uri (url).LocalPath);
				Stream stream = File.OpenRead (fname);

				long real_size = stream.Length;
				int n;
				if (compress) {
					byte[] cbuffer = new byte [8192];
					MemoryStream ms = new MemoryStream ();
					GZipStream deflate = new GZipStream (ms, CompressionMode.Compress, leaveOpen:true);
					while ((n = stream.Read (cbuffer, 0, cbuffer.Length)) != 0){
						deflate.Write (cbuffer, 0, n);
					}
					stream.Close ();
					deflate.Close ();
					byte [] bytes = ms.GetBuffer ();
					stream = new MemoryStream (bytes, 0, (int) ms.Length, false, false);
				}

				lock (monitor) {
					streams [url] = stream;
					sizes [url] = real_size;
				}
			};

			//#if NET_4_5
#if FALSE
			Parallel.ForEach (files, body);
#else
			foreach (var url in files)
				body (url);
#endif

			// The non-parallel part
			byte [] buffer = new byte [8192];
			// everything other than a-zA-Z0-9_ needs to be escaped in asm symbols.
			var symbolEscapeRE = new System.Text.RegularExpressions.Regex ("[^\\w_]");
			foreach (var url in files) {
				string fname = LocateFile (new Uri (url).LocalPath);
				string aname = MakeBundle.GetAssemblyName (fname);
				string encoded = symbolEscapeRE.Replace (aname, "_");

				if (prog == null)
					prog = aname;

				var stream = streams [url];
				var real_size = sizes [url];

				if (!quiet)
					Console.WriteLine ("   embedding: " + fname);

				WriteSymbol (ts, "assembly_data_" + encoded, stream.Length);
			
				WriteBuffer (ts, stream, buffer);

				if (compress) {
					tc.WriteLine ("extern const unsigned char assembly_data_{0} [];", encoded);
					tc.WriteLine ("static CompressedAssembly assembly_bundle_{0} = {{{{\"{1}\"," +
								  " assembly_data_{0}, {2}}}, {3}}};",
								  encoded, aname, real_size, stream.Length);
					if (!quiet) {
						double ratio = ((double) stream.Length * 100) / real_size;
						Console.WriteLine ("   compression ratio: {0:.00}%", ratio);
					}
				} else {
					tc.WriteLine ("extern const unsigned char assembly_data_{0} [];", encoded);
					tc.WriteLine ("static const MonoBundledAssembly assembly_bundle_{0} = {{\"{1}\", assembly_data_{0}, {2}}};",
								  encoded, aname, real_size);
				}
				stream.Close ();

				c_bundle_names.Add ("assembly_bundle_" + encoded);

				try {
					FileStream cf = File.OpenRead (fname + ".config");
					if (!quiet)
						Console.WriteLine (" config from: " + fname + ".config");
					tc.WriteLine ("extern const char assembly_config_{0} [];", encoded);
					WriteSymbol (ts, "assembly_config_" + encoded, cf.Length);
					WriteBuffer (ts, cf, buffer);
					ts.WriteLine ();
					config_names.Add (new string[] {aname, encoded});
				} catch (FileNotFoundException) {
					/* we ignore if the config file doesn't exist */
				}
			}

			if (config_file != null){
				FileStream conf;
				try {
					conf = File.OpenRead (config_file);
				} catch {
					Error ("Failure to open {0}", config_file);
					return;
				}
				if (!quiet)
					Console.WriteLine ("System config from: " + config_file);
				tc.WriteLine ("extern const char system_config;");
				WriteSymbol (ts, "system_config", config_file.Length);

				WriteBuffer (ts, conf, buffer);
				// null terminator
				ts.Write ("\t.byte 0\n");
				ts.WriteLine ();
			}

			if (machine_config_file != null){
				FileStream conf;
				try {
					conf = File.OpenRead (machine_config_file);
				} catch {
					Error ("Failure to open {0}", machine_config_file);
					return;
				}
				if (!quiet)
					Console.WriteLine ("Machine config from: " + machine_config_file);
				tc.WriteLine ("extern const char machine_config;");
				WriteSymbol (ts, "machine_config", machine_config_file.Length);

				WriteBuffer (ts, conf, buffer);
				ts.Write ("\t.byte 0\n");
				ts.WriteLine ();
			}
			ts.Close ();

			// Managed assemblies baked in
			if (compress)
				tc.WriteLine ("\nstatic const CompressedAssembly *compressed [] = {");
			else
				tc.WriteLine ("\nstatic const MonoBundledAssembly *bundled [] = {");

			foreach (string c in c_bundle_names){
				tc.WriteLine ("\t&{0},", c);
			}
			tc.WriteLine ("\tNULL\n};\n");

			// This must go before any attempt to access `mono_api`
			using (template_stream = System.Reflection.Assembly.GetAssembly (typeof(MakeBundle)).GetManifestResourceStream ("template_common.inc")) {
				using (s = new StreamReader (template_stream)) {
					tc.Write (s.ReadToEnd ());
				}
			}

			// AOT baked in plus loader
			foreach (string asm in aot_names){
				tc.WriteLine ("\textern const void *mono_aot_module_{0}_info;", asm);
			}

			tc.WriteLine ("\n#ifndef USE_COMPRESSED_ASSEMBLY\n");
			tc.WriteLine ("static void install_aot_modules (void) {\n");
			foreach (string asm in aot_names){
				tc.WriteLine ("\tmono_api.mono_aot_register_module (mono_aot_module_{0}_info);\n", asm);
			}

			string enum_aot_mode;
			switch (aot_mode) {
			case "hybrid":
				enum_aot_mode = "MONO_AOT_MODE_HYBRID";
				break;
			case "full":
				enum_aot_mode = "MONO_AOT_MODE_FULL";
				break;
			case "llvmonly": 
				enum_aot_mode = "MONO_AOT_MODE_LLVMONLY";
				break;
			case "": 
				enum_aot_mode = "MONO_AOT_MODE_NORMAL";
				break;
			default:
				throw new Exception ("Unsupported AOT mode");
			}
			tc.WriteLine ("\tmono_api.mono_jit_set_aot_mode ({0});", enum_aot_mode);

			tc.WriteLine ("\n}\n");
			tc.WriteLine ("#endif\n");


			tc.WriteLine ("static char *image_name = \"{0}\";", prog);

			if (ctor_func != null) {
				tc.WriteLine ("\nextern void {0} (void);", ctor_func);
				tc.WriteLine ("\n__attribute__ ((constructor)) static void mono_mkbundle_ctor (void)");
				tc.WriteLine ("{{\n\t{0} ();\n}}", ctor_func);
			}

			tc.WriteLine ("\nstatic void install_dll_config_files (void) {\n");
			foreach (string[] ass in config_names){
				tc.WriteLine ("\tmono_api.mono_register_config_for_assembly (\"{0}\", assembly_config_{1});\n", ass [0], ass [1]);
			}
			if (config_file != null)
				tc.WriteLine ("\tmono_api.mono_config_parse_memory (&system_config);\n");
			if (machine_config_file != null)
				tc.WriteLine ("\tmono_api.mono_register_machine_config (&machine_config);\n");
			tc.WriteLine ("}\n");

			if (config_dir != null)
				tc.WriteLine ("static const char *config_dir = \"{0}\";", config_dir);
			else
				tc.WriteLine ("static const char *config_dir = NULL;");

			if (compress) {
				template_stream = System.Reflection.Assembly.GetAssembly (typeof(MakeBundle)).GetManifestResourceStream ("template_z.c");
			} else {
				template_stream = System.Reflection.Assembly.GetAssembly (typeof(MakeBundle)).GetManifestResourceStream ("template.c");
			}

			using (s = new StreamReader (template_stream)) {
				tc.Write (s.ReadToEnd ());
			}
			template_stream.Dispose ();

			if (!nomain && custom_main == null) {
				Stream template_main_stream = System.Reflection.Assembly.GetAssembly (typeof(MakeBundle)).GetManifestResourceStream ("template_main.c");
				StreamReader st = new StreamReader (template_main_stream);
				string maintemplate = st.ReadToEnd ();
				tc.Write (maintemplate);
			}

			tc.Close ();

			string as_cmd = GetAssemblerCommand (temp_s, temp_o);
			Execute(as_cmd);

			if (compile_only)
				return;

			if (!quiet)
				Console.WriteLine("Compiling:");

			if (style == "windows") {
				bool staticLinkCRuntime = GetEnv ("VCCRT", "MD") != "MD";
				ToolchainProgram compiler = GetCCompiler (static_link, staticLinkCRuntime);
				if (!nomain || custom_main != null) {
					string cl_cmd = GetCompileAndLinkCommand (compiler, temp_c, temp_o, custom_main, static_link, staticLinkCRuntime, output);
					Execute (cl_cmd);
				} else {
					string temp_c_o = "";
					try {
						string cl_cmd = GetLibrarianCompilerCommand (compiler, temp_c, static_link, staticLinkCRuntime, out temp_c_o);
						Execute(cl_cmd);

						ToolchainProgram librarian = GetLibrarian ();
						string lib_cmd = GetLibrarianLinkerCommand (librarian, new string[] { temp_o, temp_c_o }, static_link, staticLinkCRuntime, output);
						Execute (lib_cmd);
					} finally {
						File.Delete (temp_c_o);
					}

				}
			}
			else
			{
				string zlib = (compress ? "-lz" : "");
				string objc = (style == "osx" ? "-framework CoreFoundation -lobjc" : "");
				string debugging = "-g";
				string cc = GetEnv("CC", "cc");
				string cmd = null;

				if (style == "linux")
					debugging = "-ggdb";
				if (static_link)
				{
					string platform_libs;
					string smonolib;

					if (style == "osx") {
						smonolib = "`pkg-config --variable=libdir mono-2`/libmono-2.0.a ";
						platform_libs = "-liconv -framework Foundation ";
					} else {
						smonolib = "-Wl,-Bstatic -lmono-2.0 -Wl,-Bdynamic ";
						platform_libs = "";
					}

					string in_tree_include = "";
					
					if (in_tree != null) {
						smonolib = String.Format ("{0}/mono/mini/.libs/libmonosgen-2.0.a", in_tree);
						in_tree_include = String.Format (" -I{0} ", in_tree);
					}

					cmd = String.Format("{4} -o '{2}' -Wall {7} `pkg-config --cflags mono-2` {9} {0} {3} " +
						"`pkg-config --libs-only-L mono-2` {5} {6} {8} " +
						"`pkg-config --libs-only-l mono-2 | sed -e \"s/\\-lmono-2.0 //\"` {1} -g ",
						temp_c, temp_o, output, zlib, cc, smonolib, String.Join (" ", aot_paths), objc, platform_libs, in_tree_include);
				}
				else
				{

					cmd = String.Format("{4} " + debugging + " -o '{2}' -Wall {5} {0} `pkg-config --cflags --libs mono-2` {3} {1}",
						temp_c, temp_o, output, zlib, cc, objc);
				}
				Execute (cmd);
			}

			if (!quiet)
				Console.WriteLine ("Done");
		}
	}
		} finally {
			if (!keeptemp){
				if (object_out == null){
					File.Delete (temp_o);
				}
				if (!compile_only){
					File.Delete (temp_c);
				}
				if (aot_temp_dir != null)
					aot_temp_dir.Delete (true);
				File.Delete (temp_s);
			}
		}
	}
	
	static List<string> LoadAssemblies (List<string> sources)
	{
		List<string> assemblies = new List<string> ();
		bool error = false;

		foreach (string name in sources){
			try {
				Assembly a = LoadAssemblyFile (name);

				if (a == null){
					error = true;
					continue;
				}
			
				assemblies.Add (a.CodeBase);
			} catch (Exception) {
				if (skip_scan) {
					if (!quiet)
						Console.WriteLine ("File will not be scanned: {0}", name);
					assemblies.Add (new Uri (new FileInfo (name).FullName).ToString ());
				} else {
					throw;
				}
			}
		}

		if (error) {
			Error ("Couldn't load one or more of the assemblies.");
			Environment.Exit (1);
		}

		return assemblies;
	}

	static void LoadLocalizedAssemblies (List<string> assemblies)
	{
		var other = i18n.Select (x => "I18N." + x + (x.Length > 0 ? "." : "") + "dll");
		string error = null;

		foreach (string name in other) {
			try {
				Assembly a = LoadAssembly (name);

				if (a == null) {
					error = "Failed to load " + name;
					continue;
				}

				assemblies.Add (a.CodeBase);
			} catch (Exception) {
				if (skip_scan) {
					if (!quiet)
						Console.WriteLine ("File will not be scanned: {0}", name);
					assemblies.Add (new Uri (new FileInfo (name).FullName).ToString ());
				} else {
					throw;
				}
			}
		}

		if (error != null) {
			Console.Error.WriteLine ("Failure to load i18n assemblies, the following directories were searched for the assemblies:");
			foreach (var path in link_paths){
				Console.Error.WriteLine ("   Path: " + path);
			}
			if (custom_mode){
				Console.WriteLine ("In Custom mode, you need to provide the directory to lookup assemblies from using -L");
			}

			Error ("Couldn't load one or more of the i18n assemblies: " + error);
			Environment.Exit (1);
		}
	}

	
	static readonly Universe universe = new Universe ();
	static readonly Dictionary<string, string> loaded_assemblies = new Dictionary<string, string> ();

	public static string GetAssemblyName (string path)
	{
		string resourcePathSeparator = style == "windows" ? "\\\\" : "/";
		string name = Path.GetFileName (path);

		// A bit of a hack to support satellite assemblies. They all share the same name but
		// are placed in subdirectories named after the locale they implement. Also, all of
		// them end in .resources.dll, therefore we can use that to detect the circumstances.
		if (name.EndsWith (".resources.dll", StringComparison.OrdinalIgnoreCase)) {
			string dir = Path.GetDirectoryName (path);
			int idx = dir.LastIndexOf (Path.DirectorySeparatorChar);
			if (idx >= 0) {
				name = dir.Substring (idx + 1) + resourcePathSeparator + name;
				Console.WriteLine ($"Storing satellite assembly '{path}' with name '{name}'");
			} else if (!quiet)
				Console.WriteLine ($"Warning: satellite assembly {path} doesn't have locale path prefix, name conflicts possible");
		}

		return name;
	}

	static bool QueueAssembly (List<string> files, string codebase)
	{
		//Console.WriteLine ("CODE BASE IS {0}", codebase);
		if (files.Contains (codebase))
			return true;

		var path = new Uri(codebase).LocalPath;
		var name = GetAssemblyName (path);
		string found;
		if (loaded_assemblies.TryGetValue (name, out found)) {
			Error ("Duplicate assembly name `{0}'. Both `{1}' and `{2}' use same assembly name.", name, path, found);
			return false;
		}

		loaded_assemblies.Add (name, path);

		files.Add (codebase);
		if (!autodeps)
			return true;
		try {
			Assembly a = universe.LoadFile (path);
			if (a == null) {
				Error ("Unable to to load assembly `{0}'", path);
				return false;
			}

			foreach (AssemblyName an in a.GetReferencedAssemblies ()) {
				a = LoadAssembly (an.Name);
				if (a == null) {
					Error ("Unable to load assembly `{0}' referenced by `{1}'", an.Name, path);
					return false;
				}

				if (!QueueAssembly (files, a.CodeBase))
					return false;
			}
		} catch (Exception) {
			if (!skip_scan)
				throw;
		}

		return true;
	}

	//
	// Loads an assembly from a specific path
	//
	static Assembly LoadAssemblyFile (string assembly)
	{
		Assembly a = null;
		
		try {
			if (!quiet)
				Console.WriteLine ("Attempting to load assembly: {0}", assembly);
			a = universe.LoadFile (assembly);
			if (!quiet)
				Console.WriteLine ("Assembly {0} loaded successfully.", assembly);

		} catch (FileNotFoundException){
			Error ($"Cannot find assembly `{assembly}'");
		} catch (IKVM.Reflection.BadImageFormatException f) {
			if (skip_scan)
				throw;
			Error ($"Cannot load assembly (bad file format) " + f.Message);
		} catch (FileLoadException f){
			Error ($"Cannot load assembly " + f.Message);
		} catch (ArgumentNullException){
			Error( $"Cannot load assembly (null argument)");
		}
		return a;
	}

	//
	// Loads an assembly from any of the link directories provided
	//
	static Assembly LoadAssembly (string assembly)
	{
		string total_log = "";
		foreach (string dir in link_paths){
			string full_path = Path.Combine (dir, assembly);
			if (!quiet)
				Console.WriteLine ("Attempting to load assembly from: " + full_path);
			if (!assembly.EndsWith (".dll") && !assembly.EndsWith (".exe"))
				full_path += ".dll";
			
			try {
				var a = universe.LoadFile (full_path);
				return a;
			} catch (FileNotFoundException ff) {
				total_log += ff.FusionLog;
				continue;
			}
		}
		if (!quiet)
			Console.WriteLine ("Log: \n" + total_log);
		return null;
	}
	
	static void Error (string msg, params object [] args)
	{
		Console.Error.WriteLine ("ERROR: {0}", string.Format (msg, args));
		Environment.Exit (1);
	}

	static void Help ()
	{
		Console.WriteLine ("Usage is: mkbundle [options] assembly1 [assembly2...]\n\n" +
				   "Options:\n" +
				   "    --config F           Bundle system config file `F'\n" +
				   "    --config-dir D       Set MONO_CFG_DIR to `D'\n" +
				   "    --deps               Turns on automatic dependency embedding (default on simple)\n" +
				   "    -L path              Adds `path' to the search path for assemblies\n" +
				   "    --machine-config F   Use the given file as the machine.config for the application.\n" +
				   "    -o out               Specifies output filename\n" +
				   "    --nodeps             Turns off automatic dependency embedding (default on custom)\n" +
				   "    --skip-scan          Skip scanning assemblies that could not be loaded (but still embed them).\n" +
				   "    --i18n ENCODING      none, all or comma separated list of CJK, MidWest, Other, Rare, West.\n" +
				   "    -v                   Verbose output\n" + 
				   "    --bundled-header     Do not attempt to include 'mono-config.h'. Define the entry points directly in the generated code\n" +
				   "\n" + 
				   "--simple   Simple mode does not require a C toolchain and can cross compile\n" + 
				   "    --cross TARGET       Generates a binary for the given TARGET\n"+
				   "    --env KEY=VALUE      Hardcodes an environment variable for the target\n" +
				   "    --fetch-target NAME  Downloads the target SDK from the remote server\n" + 
				   "    --library [LIB,]PATH Bundles the specified dynamic library to be used at runtime\n" +
				   "                         LIB is optional shortname for file located at PATH\n" + 
				   "    --list-targets       Lists available targets on the remote server\n" +
				   "    --local-targets      Lists locally available targets\n" +
				   "    --options OPTIONS    Embed the specified Mono command line options on target\n" +
				   "    --runtime RUNTIME    Manually specifies the Mono runtime to use\n" +
				   "    --sdk PATH           Use a Mono SDK root location instead of a target\n" + 
				   "    --target-server URL  Specified a server to download targets from, default is " + target_server + "\n" +
				   "\n" +
				   "--custom   Builds a custom launcher, options for --custom\n" +
				   "    -c                   Produce stub only, do not compile\n" +
				   "    -oo obj              Specifies output filename for helper object file\n" +
				   "    --dos2unix[=true|false]\n" +
				   "                         When no value provided, or when `true` specified\n" +
				   "                         `dos2unix` will be invoked to convert paths on Windows.\n" +
				   "                         When `--dos2unix=false` used, dos2unix is NEVER used.\n" +
				   "    --keeptemp           Keeps the temporary files\n" +
				   "    --static             Statically link to mono libs\n" +
				   "    --nomain             Don't include a main() function, for libraries\n" +
				   "	--custom-main C      Link the specified compilation unit (.c or .obj) with entry point/init code\n" +
				   "    -z                   Compress the assemblies before embedding.\n" +
				   "    --static-ctor ctor   Add a constructor call to the supplied function.\n" +
				   "                         You need zlib development headers and libraries.\n");
	}

	[DllImport ("libc")]
	static extern int system (string s);
	[DllImport ("libc")]
	static extern int uname (IntPtr buf);
		
	static void DetectOS ()
	{
		if (!IsUnix) {
			os_message = "OS is: Windows";
			style = "windows";
			return;
		}

		IntPtr buf = Marshal.AllocHGlobal (8192);
		if (uname (buf) != 0){
			os_message = "Warning: Unable to detect OS";
			Marshal.FreeHGlobal (buf);
			return;
		}
		string os = Marshal.PtrToStringAnsi (buf);
		os_message = "OS is: " + os;
		if (os == "Darwin")
			style = "osx";
		
		Marshal.FreeHGlobal (buf);
	}

	static bool IsUnix {
		get {
			int p = (int) Environment.OSVersion.Platform;
			return ((p == 4) || (p == 128) || (p == 6));
		}
	}


	static string EncodeAotSymbol (string symbol)
	{
		var sb = new StringBuilder ();
		/* This mimics what the aot-compiler does */
		foreach (var b in System.Text.Encoding.UTF8.GetBytes (symbol)) {
			char c = (char) b;
			if ((c >= '0' && c <= '9') ||
				(c >= 'a' && c <= 'z') ||
				(c >= 'A' && c <= 'Z')) {
				sb.Append (c);
				continue;
			}
			sb.Append ('_');
		}
		return sb.ToString ();
	}

	static void AotCompile (List<string> files)
	{
		if (aot_runtime == null)
			aot_runtime = runtime;

		if (aot_runtime == null) {
			Error ("You must specify at least one aot runtime with --runtime or --cross or --aot_runtime when AOT compiling");
			Environment.Exit (1);
		}

		var aot_mode_string = "";
		if (aot_mode != null)
			aot_mode_string = "," + aot_mode;

		var dedup_mode_string = "";
		StringBuilder all_assemblies = null;
		if (aot_dedup_assembly != null) {
			dedup_mode_string = ",dedup-skip";
			all_assemblies = new StringBuilder("");
		}

		Console.WriteLine ("Aoting files:");

		for (int i=0; i < files.Count; i++) {
			var file_name = files [i];
			string path = LocateFile (new Uri (file_name).LocalPath);
			string outPath = String.Format ("{0}.aot_out", path);
			aot_paths.Add (outPath);
			var name = System.Reflection.Assembly.LoadFrom(path).GetName().Name;
			aot_names.Add (EncodeAotSymbol (name));

			if (aot_dedup_assembly != null) {
				all_assemblies.Append (path);
				all_assemblies.Append (" ");
				Execute (String.Format ("MONO_PATH={0} {1} --aot={2},outfile={3}{4}{5} {6}",
					Path.GetDirectoryName (path), aot_runtime, aot_args, outPath, aot_mode_string, dedup_mode_string, path));
			} else {
				Execute (String.Format ("MONO_PATH={0} {1} --aot={2},outfile={3}{4} {5}",
					Path.GetDirectoryName (path), aot_runtime, aot_args, outPath, aot_mode_string, path));
			}
		}
		if (aot_dedup_assembly != null) {
			var filePath = new Uri (aot_dedup_assembly).LocalPath;
			string path = LocateFile (filePath);
			dedup_mode_string = String.Format (",dedup-include={0}", Path.GetFileName(filePath));
			string outPath = String.Format ("{0}.aot_out", path);
			Execute (String.Format ("MONO_PATH={7} {0} --aot={1},outfile={2}{3}{4} {5} {6}",
				aot_runtime, aot_args, outPath, aot_mode_string, dedup_mode_string, path, all_assemblies.ToString (), Path.GetDirectoryName (path)));
		}

		if ((aot_mode == "full" || aot_mode == "llvmonly" || aot_mode == "hybrid") && cil_strip_path != null) {
			for (int i=0; i < files.Count; i++) {
				var in_name = new Uri (files [i]).LocalPath;
				var cmd = String.Format ("{0} {1} {2}", aot_runtime, cil_strip_path, in_name);
				Execute (cmd);
			}
		}
	}

	static void LinkManaged (List <string> files, string outDir)
	{
		if (managed_linker_path == null)
			return;

		var paths = new StringBuilder ("");
		foreach (var file in files) {
			paths.Append (" -a  ");
			paths.Append (new Uri (file).LocalPath);
		}

		var cmd = String.Format ("{0} {1} -b true -out {2} {3} -c link -p copy ", runtime, managed_linker_path, outDir, paths.ToString ());
		Execute (cmd);
	}

	static void PreprocessAssemblies (List <string> chosenFiles, List <string> files)
	{
		if (aot_mode == "" || (cil_strip_path == null && managed_linker_path == null))
			return;

		var temp_dir_name = Path.Combine(Directory.GetCurrentDirectory(), "temp_assemblies");
		aot_temp_dir = new DirectoryInfo (temp_dir_name);
		if (aot_temp_dir.Exists) {
			Console.WriteLine ("Removing previous build cache at {0}", temp_dir_name);
			aot_temp_dir.Delete (true);
		}
		aot_temp_dir.Create ();

		//if (managed_linker_path != null) {
			//LinkManaged (chosenFiles, temp_dir);

			//// Replace list with new list of files
			//files.Clear ();
			//Console.WriteLine ("Iterating {0}", temp_dir);
			//aot_temp_dir = new DirectoryInfo (temp_dir);
			//foreach (var file in aot_temp_dir.GetFiles ()) {
				//files.Append (String.Format ("file:///{0}", file));
				//Console.WriteLine (String.Format ("file:///{0}", file));
			//}
			//return;
		//}

		// Fix file references
		for (int i=0; i < files.Count; i++) {
			var in_name = new Uri (files [i]).LocalPath;
			var out_name = Path.Combine (temp_dir_name, Path.GetFileName (in_name));
			File.Copy (in_name, out_name);
			files [i] = out_name;
			if (in_name == aot_dedup_assembly)
				aot_dedup_assembly = out_name;
		}
	}


	static void Execute (string cmdLine)
	{
		if (IsUnix) {
			if (!quiet)
				Console.WriteLine ("[execute cmd]: " + cmdLine);
			int ret = system (cmdLine);
			if (ret != 0)
			{
				Error(String.Format("[Fail] {0}", ret));
			}
			return;
		}

		// on Windows, we have to pipe the output of a
		// `cmd` interpolation to dos2unix, because the shell does not
		// strip the CRLFs generated by the native pkg-config distributed
		// with Mono.
		//
		// But if it's *not* on cygwin, just skip it.

		// check if dos2unix is applicable.
		if (use_dos2unix == true)
			try {
			var info = new ProcessStartInfo ("dos2unix");
			info.CreateNoWindow = true;
			info.RedirectStandardInput = true;
			info.UseShellExecute = false;
			var dos2unix = Process.Start (info);
			dos2unix.StandardInput.WriteLine ("aaa");
			dos2unix.StandardInput.WriteLine ("\u0004");
			dos2unix.StandardInput.Close ();
			dos2unix.WaitForExit ();
			if (dos2unix.ExitCode == 0)
				use_dos2unix = true;
		} catch {
			Console.WriteLine("Warning: dos2unix not found");
			use_dos2unix = false;
		}

		if (use_dos2unix == null)
			use_dos2unix = false;

		ProcessStartInfo psi = new ProcessStartInfo();
		psi.UseShellExecute = false;

		// if there is no dos2unix, just run cmd /c.
		if (use_dos2unix == false)
		{
			psi.FileName = "cmd";
			psi.Arguments = String.Format("/c \"{0}\"", cmdLine);
		}
		else
		{
			psi.FileName = "sh";
			StringBuilder b = new StringBuilder();
			int count = 0;
			for (int i = 0; i < cmdLine.Length; i++)
			{
				if (cmdLine[i] == '`')
				{
					if (count % 2 != 0)
					{
						b.Append("|dos2unix");
					}
					count++;
				}
				b.Append(cmdLine[i]);
			}
			cmdLine = b.ToString();
			psi.Arguments = String.Format("-c \"{0}\"", cmdLine);
		}

		if (!quiet)
			Console.WriteLine(cmdLine);
		using (Process p = Process.Start (psi)) {
			p.WaitForExit ();
			int ret = p.ExitCode;
			if (ret != 0){
				Error ("[Fail] {0}", ret);
			}
		}
	}

	static string GetEnv(string name, string defaultValue)
	{
		string val = Environment.GetEnvironmentVariable(name);
		if (val != null)
		{
			if (!quiet)
				Console.WriteLine("{0} = {1}", name, val);
		}
		else
		{
			val = defaultValue;
			if (!quiet)
				Console.WriteLine("{0} = {1} (default)", name, val);
		}
		return val;
	}

	static string LocateFile(string default_path)
	{
		var override_path = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(default_path));
		if (File.Exists(override_path))
			return override_path;
		else if (File.Exists(default_path))
			return default_path;
		else
			throw new FileNotFoundException(default_path);
	}

	static string GetAssemblerCommand (string sourceFile, string objectFile)
	{
		if (style == "windows") {
			string additionalArguments = "";
			var assembler = GetAssemblerCompiler ();
			if (assembler.Name.Contains ("clang.exe"))
				//Clang uses additional arguments.
				additionalArguments = "-c -x assembler";

			return String.Format ("\"{0}\" {1} -o {2} {3} ", assembler.Path, additionalArguments, objectFile, sourceFile);
		} else {
			return String.Format ("{0} -o {1} {2} ", GetEnv ("AS", "as"), objectFile, sourceFile);
		}
	}

#region WindowsToolchainSupport

	class StringVersionComparer : IComparer<string> {
		public int Compare (string stringA, string stringB)
		{
			Version versionA;
			Version versionB;

			var versionAMatch = System.Text.RegularExpressions.Regex.Match (stringA, @"\d+(\.\d +) + ");
			if (versionAMatch.Success)
				stringA = versionAMatch.ToString ();

			var versionBMatch = System.Text.RegularExpressions.Regex.Match (stringB, @"\d+(\.\d+)+");
			if (versionBMatch.Success)
				stringB = versionBMatch.ToString ();

			if (Version.TryParse (stringA, out versionA) && Version.TryParse (stringB, out versionB))
				return versionA.CompareTo (versionB);

			return string.Compare (stringA, stringB, StringComparison.OrdinalIgnoreCase);
		}
	}

	class InstalledSDKInfo {
		public InstalledSDKInfo (string name, string version, string installationFolder)
		{
			this.Name = name;
			this.Version = Version.Parse (version);
			this.InstallationFolder = installationFolder;
			this.AdditionalSDKs = new List<InstalledSDKInfo> ();
		}

		public InstalledSDKInfo (string name, string version, string installationFolder, bool isSubVersion)
			: this (name, version, installationFolder)
		{
			this.IsSubVersion = isSubVersion;
		}

		public InstalledSDKInfo (string name, string version, string installationFolder, bool isSubVersion, InstalledSDKInfo parentSDK)
			: this (name, version, installationFolder, isSubVersion)
		{
			this.ParentSDK = parentSDK;
		}

		public string Name { get; set; }
		public Version Version { get; set; }
		public string InstallationFolder { get; set; }
		public bool IsSubVersion { get; set; }
		public List<InstalledSDKInfo> AdditionalSDKs { get; }
		public InstalledSDKInfo ParentSDK { get; set; }
	}

	class ToolchainProgram {
		public ToolchainProgram (string name, string path)
		{
			this.Name = name;
			this.Path = path;
		}

		public ToolchainProgram (string name, string path, InstalledSDKInfo parentSDK)
			: this (name, path)
		{
			this.ParentSDK = parentSDK;
		}

		public Func<string, string> QuoteArg = arg => "\"" + arg + "\"";
		public string Name { get; set; }
		public string Path { get; set; }
		public InstalledSDKInfo ParentSDK { get; set; }
		public bool IsVSToolChain { get { return (Name.Contains ("cl.exe") || Name.Contains ("lib.exe")); } }
		public bool IsGCCToolChain { get { return !IsVSToolChain;  } }
	}

	class SDKHelper {
		static protected Microsoft.Win32.RegistryKey GetToolchainRegistrySubKey (string subKey)
		{
			Microsoft.Win32.RegistryKey key = null;

			if (Environment.Is64BitProcess) {
				key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (@"SOFTWARE\Wow6432Node" + subKey) ??
					Microsoft.Win32.Registry.CurrentUser.OpenSubKey (@"SOFTWARE\Wow6432Node" + subKey);
			}

			if (key == null) {
				key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (@"SOFTWARE" + subKey) ??
					Microsoft.Win32.Registry.CurrentUser.OpenSubKey (@"SOFTWARE" + subKey);
			}

			return key;
		}
	}

	class WindowsSDKHelper : SDKHelper {
		List<InstalledSDKInfo> installedWindowsSDKs;
		List<InstalledSDKInfo> installedCRuntimeSDKs;
		InstalledSDKInfo installedWindowsSDK;
		InstalledSDKInfo installedCRuntimeSDK;

		static WindowsSDKHelper singletonInstance = new WindowsSDKHelper ();
		static public WindowsSDKHelper GetInstance ()
		{
			return singletonInstance;
		}

		Dictionary<string, string> GetInstalledWindowsKitRootFolders ()
		{
			var rootFolders = new Dictionary<string, string> ();

			using (var subKey = GetToolchainRegistrySubKey (@"\Microsoft\Microsoft SDKs\Windows\")) {
				if (subKey != null) {
					foreach (var keyName in subKey.GetSubKeyNames ()) {
						var keyNameIsVersion = System.Text.RegularExpressions.Regex.Match (keyName, @"\d+(\.\d+)+");
						if (keyNameIsVersion.Success) {
							var installFolder = (string)Microsoft.Win32.Registry.GetValue (subKey.ToString () + @"\" + keyName, "InstallationFolder", "");
							if (!rootFolders.ContainsKey (installFolder))
								rootFolders.Add (installFolder, keyNameIsVersion.ToString ());
						}
					}
				}
			}

			using (var subKey = GetToolchainRegistrySubKey (@"\Microsoft\Windows Kits\Installed Roots")) {
				if (subKey != null) {
					foreach (var valueName in subKey.GetValueNames ()) {
						var valueNameIsKitsRoot = System.Text.RegularExpressions.Regex.Match (valueName, @"KitsRoot\d*");
						if (valueNameIsKitsRoot.Success) {
							var installFolder = (string)Microsoft.Win32.Registry.GetValue (subKey.ToString (), valueName, "");
							if (!rootFolders.ContainsKey (installFolder)) {
								var valueNameIsVersion = System.Text.RegularExpressions.Regex.Match (valueName, @"\d+(\.*\d+)+");
								if (valueNameIsVersion.Success)
									rootFolders.Add (installFolder, valueNameIsVersion.ToString ());
								else
									rootFolders.Add (installFolder, "");
							}
						}
					}
				}
			}

			return rootFolders;
		}

		void InitializeInstalledWindowsKits ()
		{
			if (installedWindowsSDKs == null && installedCRuntimeSDKs == null) {
				List<InstalledSDKInfo> windowsSDKs = new List<InstalledSDKInfo> ();
				List<InstalledSDKInfo> cRuntimeSDKs = new List<InstalledSDKInfo> ();
				var rootFolders = GetInstalledWindowsKitRootFolders ();
				foreach (var winKitRoot in rootFolders) {
					// Try to locate Windows and CRuntime SDKs.
					string winKitRootDir = winKitRoot.Key;
					string winKitRootVersion = winKitRoot.Value;
					string winKitIncludeDir = Path.Combine (winKitRootDir, "include");

					//Search for installed SDK versions.
					if (Directory.Exists (winKitIncludeDir)) {
						var winKitIncludeDirInfo = new DirectoryInfo (winKitIncludeDir);
						var versions = winKitIncludeDirInfo.GetDirectories ("*.*", SearchOption.TopDirectoryOnly)
							.OrderByDescending (p => p.Name, new StringVersionComparer ());

						foreach (var version in versions) {
							string versionedWindowsSDKHeaderPath = Path.Combine (version.FullName, "um", "windows.h");
							string versionedCRuntimeSDKHeaderPath = Path.Combine (version.FullName, "ucrt", "stdlib.h");
							var hasSubVersion = System.Text.RegularExpressions.Regex.Match (version.Name, @"\d+(\.\d+)+");
							if (hasSubVersion.Success) {
								if (File.Exists (versionedWindowsSDKHeaderPath))
									//Found a specific Windows SDK sub version.
									windowsSDKs.Add (new InstalledSDKInfo ("WindowsSDK", hasSubVersion.ToString (), winKitRootDir, true));
								if (File.Exists (versionedCRuntimeSDKHeaderPath))
									//Found a specific CRuntime SDK sub version.
									cRuntimeSDKs.Add (new InstalledSDKInfo ("CRuntimeSDK", hasSubVersion.ToString (), winKitRootDir, true));
							}
						}
					}

					// Try to find SDK without specific sub version.
					string windowsSDKHeaderPath = Path.Combine (winKitIncludeDir, "um", "windows.h");
					if (File.Exists (windowsSDKHeaderPath))
						//Found a Windows SDK version.
						windowsSDKs.Add (new InstalledSDKInfo ("WindowsSDK", winKitRootVersion, winKitRootDir, false));

					string cRuntimeSDKHeaderPath = Path.Combine (winKitIncludeDir, "ucrt", "stdlib.h");
					if (File.Exists (cRuntimeSDKHeaderPath))
						//Found a CRuntime SDK version.
						cRuntimeSDKs.Add (new InstalledSDKInfo ("CRuntimeSDK", winKitRootVersion, winKitRootDir, false));
				}

				// Sort based on version.
				windowsSDKs = windowsSDKs.OrderByDescending (p => p.Version.ToString (), new StringVersionComparer ()).ToList ();
				cRuntimeSDKs = cRuntimeSDKs.OrderByDescending (p => p.Version.ToString (), new StringVersionComparer ()).ToList ();

				installedWindowsSDKs = windowsSDKs;
				installedCRuntimeSDKs = cRuntimeSDKs;

				if (!quiet && installedWindowsSDKs != null) {
					Console.WriteLine ("--- Windows SDK's ---");
					foreach (var windowsSDK in installedWindowsSDKs) {
						Console.WriteLine ("Path: " + windowsSDK.InstallationFolder);
						Console.WriteLine ("Version: " + windowsSDK.Version);
					}
					Console.WriteLine ("---------------");
				}

				if (!quiet && installedCRuntimeSDKs != null) {
					Console.WriteLine ("--- C-Runtime SDK's ---");
					foreach (var cRuntimeSDK in installedCRuntimeSDKs) {
						Console.WriteLine ("Path: " + cRuntimeSDK.InstallationFolder);
						Console.WriteLine ("Version: " + cRuntimeSDK.Version);
						if (cRuntimeSDK.ParentSDK != null) {
							Console.WriteLine ("Parent SDK Path: " + cRuntimeSDK.ParentSDK.InstallationFolder);
							Console.WriteLine ("Parent SDK Version: " + cRuntimeSDK.ParentSDK.Version);
						}
					}
					Console.WriteLine ("---------------");
				}
			}

			return;
		}

		List<InstalledSDKInfo> GetInstalledWindowsSDKs ()
		{
			if (installedWindowsSDKs == null)
				InitializeInstalledWindowsKits ();

			return installedWindowsSDKs;
		}

		List<InstalledSDKInfo> GetInstalledCRuntimeSDKs ()
		{
			if (installedCRuntimeSDKs == null)
				InitializeInstalledWindowsKits ();

			return installedCRuntimeSDKs;
		}

		InstalledSDKInfo GetInstalledWindowsSDK ()
		{
			if (installedWindowsSDK == null) {
				string winSDKDir = "";
				InstalledSDKInfo windowsSDK = null;
				List<InstalledSDKInfo> windowsSDKs = GetInstalledWindowsSDKs ();

				// Check that env doesn't already include needed values.
				winSDKDir = GetEnv ("WINSDK", "");
				if (winSDKDir.Length == 0)
					// If executed from a VS developer command prompt, SDK dir set in env.
					winSDKDir = GetEnv ("WindowsSdkDir", "");

				// Check that env doesn't already include needed values.
				// If executed from a VS developer command prompt, SDK version set in env.
				var winSDKVersion = System.Text.RegularExpressions.Regex.Match (GetEnv ("WindowsSdkVersion", ""), @"\d+(\.\d+)+");

				if (winSDKDir.Length != 0 && windowsSDKs != null) {
					// Find installed SDK based on requested info.
					if (winSDKVersion.Success)
						windowsSDK = windowsSDKs.Find (x => (x.InstallationFolder == winSDKDir && x.Version.ToString () == winSDKVersion.ToString ()));
					else
						windowsSDK = windowsSDKs.Find (x => x.InstallationFolder == winSDKDir);
				}

				if (windowsSDK == null && winSDKVersion.Success && windowsSDKs != null)
					// Find installed SDK based on requested info.
					windowsSDK = windowsSDKs.Find (x => x.Version.ToString () == winSDKVersion.ToString ());

				if (windowsSDK == null && windowsSDKs != null)
					// Get latest installed verison.
					windowsSDK = windowsSDKs.First ();

				installedWindowsSDK = windowsSDK;
			}

			return installedWindowsSDK;
		}

		string FindCRuntimeSDKIncludePath (InstalledSDKInfo sdk)
		{
			string cRuntimeIncludePath = Path.Combine (sdk.InstallationFolder, "include");
			if (sdk.IsSubVersion)
				cRuntimeIncludePath = Path.Combine (cRuntimeIncludePath, sdk.Version.ToString ());

			cRuntimeIncludePath = Path.Combine (cRuntimeIncludePath, "ucrt");
			if (!Directory.Exists (cRuntimeIncludePath))
				cRuntimeIncludePath = "";

			return cRuntimeIncludePath;
		}

		string FindCRuntimeSDKLibPath (InstalledSDKInfo sdk)
		{
			string cRuntimeLibPath = Path.Combine (sdk.InstallationFolder, "lib");
			if (sdk.IsSubVersion)
				cRuntimeLibPath = Path.Combine (cRuntimeLibPath, sdk.Version.ToString ());

			cRuntimeLibPath = Path.Combine (cRuntimeLibPath, "ucrt", Target64BitApplication () ? "x64" : "x86");
			if (!Directory.Exists (cRuntimeLibPath))
				cRuntimeLibPath = "";

			return cRuntimeLibPath;
		}

		InstalledSDKInfo GetInstalledCRuntimeSDK ()
		{
			if (installedCRuntimeSDK == null) {
				InstalledSDKInfo cRuntimeSDK = null;
				var windowsSDK = GetInstalledWindowsSDK ();
				var cRuntimeSDKs = GetInstalledCRuntimeSDKs ();

				if (windowsSDK != null && cRuntimeSDKs != null) {
					cRuntimeSDK = cRuntimeSDKs.Find (x => x.Version.ToString () == windowsSDK.Version.ToString ());
					if (cRuntimeSDK == null && cRuntimeSDKs.Count != 0)
						cRuntimeSDK = cRuntimeSDKs.First ();

					installedCRuntimeSDK = cRuntimeSDK;
				}
			}

			return installedCRuntimeSDK;
		}

		public void AddWindowsSDKIncludePaths (List<string> includePaths)
		{
			InstalledSDKInfo winSDK = GetInstalledWindowsSDK ();
			if (winSDK != null) {
				string winSDKIncludeDir = Path.Combine (winSDK.InstallationFolder, "include");

				if (winSDK.IsSubVersion)
					winSDKIncludeDir = Path.Combine (winSDKIncludeDir, winSDK.Version.ToString ());

				// Include sub folders.
				if (Directory.Exists (winSDKIncludeDir)) {
					includePaths.Add (Path.Combine (winSDKIncludeDir, "um"));
					includePaths.Add (Path.Combine (winSDKIncludeDir, "shared"));
					includePaths.Add (Path.Combine (winSDKIncludeDir, "winrt"));
				}
			}

			return;
		}

		public void AddWindowsSDKLibPaths (List<string> libPaths)
		{
			InstalledSDKInfo winSDK = GetInstalledWindowsSDK ();
			if (winSDK != null) {
				string winSDKLibDir = Path.Combine (winSDK.InstallationFolder, "lib");

				if (winSDK.IsSubVersion) {
					winSDKLibDir = Path.Combine (winSDKLibDir, winSDK.Version.ToString ());
				} else {
					// Older WinSDK's header folders are not versioned, but installed libraries are, use latest available version for now.
					var winSDKLibDirInfo = new DirectoryInfo (winSDKLibDir);
					var version = winSDKLibDirInfo.GetDirectories ("*.*", SearchOption.TopDirectoryOnly)
						.OrderByDescending (p => p.Name, new StringVersionComparer ()).FirstOrDefault ();
					if (version != null)
						winSDKLibDir = version.FullName;
				}

				//Enumerat lib sub folders.
				if (Directory.Exists (winSDKLibDir))
					libPaths.Add (Path.Combine (winSDKLibDir, "um", Target64BitApplication () ? "x64" : "x86"));
			}

			return;
		}

		public void AddCRuntimeSDKIncludePaths (List<string> includePaths)
		{
			InstalledSDKInfo cRuntimeSDK = GetInstalledCRuntimeSDK ();
			if (cRuntimeSDK != null) {
				string cRuntimeSDKIncludeDir = FindCRuntimeSDKIncludePath (cRuntimeSDK);

				if (cRuntimeSDKIncludeDir.Length != 0)
					includePaths.Add (cRuntimeSDKIncludeDir);
			}

			return;
		}

		public void AddCRuntimeSDKLibPaths (List<string> libPaths)
		{
			InstalledSDKInfo cRuntimeSDK = GetInstalledCRuntimeSDK ();
			if (cRuntimeSDK != null) {
				string cRuntimeSDKLibDir = FindCRuntimeSDKLibPath (cRuntimeSDK);

				if (cRuntimeSDKLibDir.Length != 0)
					libPaths.Add (cRuntimeSDKLibDir);
			}

			return;
		}
	}

	class VisualStudioSDKHelper : SDKHelper {
		List<InstalledSDKInfo> installedVisualStudioSDKs;
		InstalledSDKInfo installedVisualStudioSDK;

		static VisualStudioSDKHelper singletonInstance = new VisualStudioSDKHelper ();
		static public VisualStudioSDKHelper GetInstance ()
		{
			return singletonInstance;
		}

		List<InstalledSDKInfo> InitializeInstalledVisualStudioSDKs ()
		{
			if (installedVisualStudioSDKs == null) {
				List<InstalledSDKInfo> sdks = new List<InstalledSDKInfo> ();

				using (var subKey = GetToolchainRegistrySubKey (@"\Microsoft\VisualStudio\SxS\VS7")) {
					if (subKey != null) {
						foreach (var keyName in subKey.GetValueNames ()) {
							var vsInstalltionFolder = (string)Microsoft.Win32.Registry.GetValue (subKey.ToString (), keyName, "");
							if (Directory.Exists (vsInstalltionFolder)) {
								var vsSDK = new InstalledSDKInfo ("VisualStudio", keyName, vsInstalltionFolder, false);
								var vcInstallationFolder = Path.Combine (vsInstalltionFolder, "VC");

								if (Directory.Exists (vcInstallationFolder))
									vsSDK.AdditionalSDKs.Add (new InstalledSDKInfo ("VisualStudioVC", keyName, vcInstallationFolder, false, vsSDK));

								sdks.Add (vsSDK);
							}
						}
					}
				}

				// TODO: Add VS15 SetupConfiguration support.
				// To reduce dependecies use vswhere.exe, if available.

				// Sort based on version.
				sdks = sdks.OrderByDescending (p => p.Version.ToString (), new StringVersionComparer ()).ToList ();
				installedVisualStudioSDKs = sdks;
			}

			return installedVisualStudioSDKs;
		}

		string FindVisualStudioVCFolderPath (InstalledSDKInfo vcSDK, string subPath)
		{
			string folderPath = "";
			if (vcSDK != null && vcSDK.ParentSDK != null) {
				if (IsVisualStudio14 (vcSDK.ParentSDK)) {
					folderPath = Path.Combine (vcSDK.InstallationFolder, subPath);
				} else if (IsVisualStudio15 (vcSDK.ParentSDK)) {
					string msvcVersionPath = Path.Combine (vcSDK.InstallationFolder, "Tools", "MSVC");

					// Add latest found version of MSVC toolchain.
					if (Directory.Exists (msvcVersionPath)) {
						var msvcVersionDirInfo = new DirectoryInfo (msvcVersionPath);
						var versions = msvcVersionDirInfo.GetDirectories ("*.*", SearchOption.TopDirectoryOnly)
							.OrderByDescending (p => p.Name, new StringVersionComparer ());

						foreach (var version in versions) {
							msvcVersionPath = Path.Combine (version.FullName, subPath);
							if (Directory.Exists (msvcVersionPath)) {
								folderPath = msvcVersionPath;
								break;
							}
						}
					}
				}
			}

			return folderPath;
		}

		string FindVisualStudioVCLibSubPath (InstalledSDKInfo vcSDK)
		{
			string subPath = "";

			if (vcSDK != null && vcSDK.ParentSDK != null) {
				if (IsVisualStudio14 (vcSDK.ParentSDK))
					subPath = Target64BitApplication () ? @"lib\amd64" : "lib";
				else if (IsVisualStudio15 (vcSDK.ParentSDK))
					subPath = Target64BitApplication () ? @"lib\x64" : @"lib\x86";
			}

			return subPath;
		}

		public InstalledSDKInfo GetInstalledVisualStudioSDK ()
		{
			if (installedVisualStudioSDK == null) {
				List<InstalledSDKInfo> visualStudioSDKs = InitializeInstalledVisualStudioSDKs ();
				InstalledSDKInfo visualStudioSDK = null;

				// Check that env doesn't already include needed values.
				// If executed from a VS developer command prompt, Visual Studio install dir set in env.
				string vsVersion = GetEnv ("VisualStudioVersion", "");

				if (vsVersion.Length != 0 && visualStudioSDKs != null)
					// Find installed SDK based on requested info.
					visualStudioSDK = visualStudioSDKs.Find (x => x.Version.ToString () == vsVersion);

				if (visualStudioSDK == null && visualStudioSDKs != null)
					// Get latest installed verison.
					visualStudioSDK = visualStudioSDKs.First ();

				installedVisualStudioSDK = visualStudioSDK;
			}

			return installedVisualStudioSDK;
		}

		public InstalledSDKInfo GetInstalledVisualStudioVCSDK ()
		{
			InstalledSDKInfo visualStudioVCSDK = null;

			// Check that env doesn't already include needed values.
			// If executed from a VS developer command prompt, Visual Studio install dir set in env.
			string vcInstallDir = GetEnv ("VCINSTALLDIR", "");
			if (vcInstallDir.Length != 0) {
				List<InstalledSDKInfo> installedVisualStudioSDKs = InitializeInstalledVisualStudioSDKs ();
				if (installedVisualStudioSDKs != null) {
					foreach (var currentInstalledSDK in installedVisualStudioSDKs) {
						// Find installed SDK based on requested info.
						visualStudioVCSDK = currentInstalledSDK.AdditionalSDKs.Find (x => x.InstallationFolder == vcInstallDir);
						if (visualStudioVCSDK != null)
							break;
					}
				}
			}

			// Get latest installed VS VC SDK version.
			if (visualStudioVCSDK == null) {
				var visualStudioSDK = GetInstalledVisualStudioSDK ();
				if (visualStudioSDK != null)
					visualStudioVCSDK = visualStudioSDK.AdditionalSDKs.Find (x => x.Name == "VisualStudioVC");
			}

			return visualStudioVCSDK;
		}

		public bool IsVisualStudio14 (InstalledSDKInfo vsSDK)
		{
			return vsSDK.Version.Major == 14 || vsSDK.Version.Major == 2015;
		}

		public bool IsVisualStudio15 (InstalledSDKInfo vsSDK)
		{
			return vsSDK.Version.Major == 15 || vsSDK.Version.Major == 2017;
		}

		public void AddVisualStudioVCIncludePaths (List<string> includePaths)
		{
			// Check that env doesn't already include needed values.
			string vcIncludeDir = GetEnv ("VSINCLUDE", "");
			if (vcIncludeDir.Length == 0) {
				var visualStudioVCSDK = GetInstalledVisualStudioVCSDK ();
				vcIncludeDir = FindVisualStudioVCFolderPath (visualStudioVCSDK, "include");
			}

			if (vcIncludeDir.Length != 0)
				includePaths.Add (vcIncludeDir);

			return;
		}

		public void AddVisualStudioVCLibPaths (List<string> libPaths)
		{
			// Check that env doesn't already include needed values.
			string vcLibDir = GetEnv ("VSLIB", "");
			if (vcLibDir.Length == 0) {
				var vcSDK = GetInstalledVisualStudioVCSDK ();
				vcLibDir = FindVisualStudioVCFolderPath (vcSDK, FindVisualStudioVCLibSubPath (vcSDK));
			}

			if (vcLibDir.Length != 0)
				libPaths.Add (vcLibDir);

			return;
		}
	}

	class VCToolchainProgram {
		protected ToolchainProgram toolchain;
		public virtual bool IsVersion (InstalledSDKInfo vcSDK) { return false; }
		public virtual ToolchainProgram FindVCToolchainProgram (InstalledSDKInfo vcSDK) { return null; }
	}

	class VC14ToolchainProgram : VCToolchainProgram {
		public override bool IsVersion (InstalledSDKInfo vcSDK)
		{
			return VisualStudioSDKHelper.GetInstance ().IsVisualStudio14 (vcSDK);
		}

		protected ToolchainProgram FindVCToolchainProgram (string tool, InstalledSDKInfo vcSDK)
		{
			if (toolchain == null) {
				string toolPath = "";
				if (!string.IsNullOrEmpty (vcSDK?.InstallationFolder)) {
					if (Target64BitApplication ())
						toolPath = Path.Combine (new string [] { vcSDK.InstallationFolder, "bin", "amd64", tool });
					else
						toolPath = Path.Combine (new string [] { vcSDK.InstallationFolder, "bin", tool });

					if (!File.Exists (toolPath))
						toolPath = "";
				}

				toolchain = new ToolchainProgram (tool, toolPath, vcSDK);
			}

			return toolchain;
		}
	}

	class VC15ToolchainProgram : VCToolchainProgram {
		public override bool IsVersion (InstalledSDKInfo vcSDK)
		{
			return VisualStudioSDKHelper.GetInstance ().IsVisualStudio15 (vcSDK);
		}

		protected ToolchainProgram FindVCToolchainProgram (string tool, InstalledSDKInfo vcSDK)
		{
			if (toolchain == null) {
				string toolPath = "";
				if (!string.IsNullOrEmpty (vcSDK?.InstallationFolder)) {
					string toolsVersionFilePath = Path.Combine (vcSDK.InstallationFolder, "Auxiliary", "Build", "Microsoft.VCToolsVersion.default.txt");
					if (File.Exists (toolsVersionFilePath)) {
						var lines = File.ReadAllLines (toolsVersionFilePath);
						if (lines.Length > 0) {
							string toolsVersionPath = Path.Combine (vcSDK.InstallationFolder, "Tools", "MSVC", lines [0].Trim ());
							if (Target64BitApplication ())
								toolPath = Path.Combine (toolsVersionPath, "bin", "HostX64", "x64", tool);
							else
								toolPath = Path.Combine (toolsVersionPath, "bin", "HostX86", "x86", tool);

							if (!File.Exists (toolPath))
								toolPath = "";
						}
					}
				}

				toolchain = new ToolchainProgram (tool, toolPath, vcSDK);
			}

			return toolchain;
		}
	}

	class VC14Compiler : VC14ToolchainProgram {
		public override ToolchainProgram FindVCToolchainProgram (InstalledSDKInfo vcSDK)
		{
			return FindVCToolchainProgram ("cl.exe", vcSDK);
		}
	}

	class VC15Compiler : VC15ToolchainProgram {
		public override ToolchainProgram FindVCToolchainProgram (InstalledSDKInfo vcSDK)
		{
			return FindVCToolchainProgram ("cl.exe", vcSDK);
		}
	}

	class VC14Librarian : VC14ToolchainProgram {
		public override ToolchainProgram FindVCToolchainProgram (InstalledSDKInfo vcSDK)
		{
			return FindVCToolchainProgram ("lib.exe", vcSDK);
		}
	}

	class VC15Librarian : VC15ToolchainProgram {
		public override ToolchainProgram FindVCToolchainProgram (InstalledSDKInfo vcSDK)
		{
			return FindVCToolchainProgram ("lib.exe", vcSDK);
		}
	}

	class VC14Clang : VCToolchainProgram {
		public override bool IsVersion (InstalledSDKInfo vcSDK)
		{
			return VisualStudioSDKHelper.GetInstance ().IsVisualStudio14 (vcSDK);
		}

		public override ToolchainProgram FindVCToolchainProgram (InstalledSDKInfo vcSDK)
		{
			if (toolchain == null) {
				string clangPath = "";
				if (!string.IsNullOrEmpty (vcSDK?.InstallationFolder)) {
					clangPath = Path.Combine (new string [] { vcSDK.InstallationFolder, "ClangC2", "bin", Target64BitApplication () ? "amd64" : "x86", "clang.exe" });

					if (!File.Exists (clangPath))
						clangPath = "";
				}

				toolchain = new ToolchainProgram ("clang.exe", clangPath, vcSDK);
			}

			return toolchain;
		}
	}

	class VC15Clang : VCToolchainProgram {
		public override bool IsVersion (InstalledSDKInfo vcSDK)
		{
			return VisualStudioSDKHelper.GetInstance ().IsVisualStudio15 (vcSDK);
		}

		public override ToolchainProgram FindVCToolchainProgram (InstalledSDKInfo vcSDK)
		{
			if (toolchain == null) {
				string clangPath = "";
				if (!string.IsNullOrEmpty (vcSDK?.InstallationFolder)) {
					string clangVersionFilePath = Path.Combine (vcSDK.InstallationFolder, "Auxiliary", "Build", "Microsoft.ClangC2Version.default.txt");
					if (File.Exists (clangVersionFilePath)) {
						var lines = File.ReadAllLines (clangVersionFilePath);
						if (lines.Length > 0) {
							string clangVersionPath = Path.Combine (vcSDK.InstallationFolder, "Tools", "ClangC2", lines [0].Trim ());

							clangPath = Path.Combine (clangVersionPath, "bin", Target64BitApplication () ? "HostX64" : "HostX86", "clang.exe");

							if (!File.Exists (clangPath))
								clangPath = "";
						}
					}
				}

				toolchain = new ToolchainProgram ("clang.exe", clangPath, vcSDK);
			}

			return toolchain;
		}
	}

	class VisualStudioSDKToolchainHelper {
		List<VCToolchainProgram> vcCompilers = new List<VCToolchainProgram> ();
		List<VCToolchainProgram> vcLibrarians = new List<VCToolchainProgram> ();
		List<VCToolchainProgram> vcClangCompilers = new List<VCToolchainProgram> ();

		public VisualStudioSDKToolchainHelper ()
		{
			vcCompilers.Add (new VC14Compiler ());
			vcCompilers.Add (new VC15Compiler ());

			vcLibrarians.Add (new VC14Librarian ());
			vcLibrarians.Add (new VC15Librarian ());

			vcClangCompilers.Add (new VC14Clang ());
			vcClangCompilers.Add (new VC15Clang ());
		}

		static VisualStudioSDKToolchainHelper singletonInstance = new VisualStudioSDKToolchainHelper ();
		static public VisualStudioSDKToolchainHelper GetInstance ()
		{
			return singletonInstance;
		}

		ToolchainProgram GetVCToolChainProgram (List<VCToolchainProgram> programs)
		{
			var vcSDK = VisualStudioSDKHelper.GetInstance ().GetInstalledVisualStudioVCSDK ();
			if (vcSDK?.ParentSDK != null) {
				foreach (var item in programs) {
					if (item.IsVersion (vcSDK.ParentSDK)) {
						return item.FindVCToolchainProgram (vcSDK);
					}
				}
			}

			return null;
		}

		public ToolchainProgram GetVCCompiler ()
		{
			return GetVCToolChainProgram (vcCompilers);
		}

		public ToolchainProgram GetVCLibrarian ()
		{
			return GetVCToolChainProgram (vcLibrarians);
		}

		public ToolchainProgram GetVCClangCompiler ()
		{
			return GetVCToolChainProgram (vcClangCompilers);
		}
	}

	static bool Target64BitApplication ()
	{
		// Should probably handled the --cross and sdk parameters.
		string targetArchitecture = GetEnv ("VSCMD_ARG_TGT_ARCH", "");
		if (targetArchitecture.Length != 0) {
			if (string.Compare (targetArchitecture, "x64", StringComparison.OrdinalIgnoreCase) == 0)
				return true;
			else
				return false;
		} else {
			return Environment.Is64BitProcess;
		}
	}

	static string GetMonoDir ()
	{
		// Check that env doesn't already include needed values.
		string monoInstallDir = GetEnv ("MONOPREFIX", "");
		if (monoInstallDir.Length == 0) {
			using (var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey (Microsoft.Win32.RegistryHive.LocalMachine,
				Target64BitApplication () ? Microsoft.Win32.RegistryView.Registry64 : Microsoft.Win32.RegistryView.Registry32)) {

				if (baseKey != null) {
					using (var subKey = baseKey.OpenSubKey (@"SOFTWARE\Mono")) {
						if (subKey != null)
							monoInstallDir = (string)subKey.GetValue ("SdkInstallRoot", "");
					}
				}
			}
		}

		return monoInstallDir;
	}

	static void AddMonoIncludePaths (List<string> includePaths)
	{
		includePaths.Add (Path.Combine (GetMonoDir (), @"include\mono-2.0"));
		return;
	}

	static void AddMonoLibPaths (List<string> libPaths)
	{
		libPaths.Add (Path.Combine (GetMonoDir (), "lib"));
		return;
	}

	static void AddIncludePaths (List<string> includePaths)
	{
		// Check that env doesn't already include needed values.
		// If executed from a VS developer command prompt, all includes are already setup in env.
		string includeEnv = GetEnv ("INCLUDE", "");
		if (includeEnv.Length == 0) {
			VisualStudioSDKHelper.GetInstance ().AddVisualStudioVCIncludePaths (includePaths);
			WindowsSDKHelper.GetInstance ().AddCRuntimeSDKIncludePaths (includePaths);
			WindowsSDKHelper.GetInstance ().AddWindowsSDKIncludePaths (includePaths);
		}

		AddMonoIncludePaths (includePaths);
		includePaths.Add (".");

		return;
	}

	static void AddLibPaths (List<string> libPaths)
	{
		// Check that env doesn't already include needed values.
		// If executed from a VS developer command prompt, all libs are already setup in env.
		string libEnv = GetEnv ("LIB", "");
		if (libEnv.Length == 0) {
			VisualStudioSDKHelper.GetInstance ().AddVisualStudioVCLibPaths (libPaths);
			WindowsSDKHelper.GetInstance ().AddCRuntimeSDKLibPaths (libPaths);
			WindowsSDKHelper.GetInstance ().AddWindowsSDKLibPaths (libPaths);
		}

		AddMonoLibPaths (libPaths);
		libPaths.Add (".");

		return;
	}

	static void AddVCSystemLibraries (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime, List<string> linkerArgs)
	{
		linkerArgs.Add ("kernel32.lib");
		linkerArgs.Add ("version.lib");
		linkerArgs.Add ("ws2_32.lib");
		linkerArgs.Add ("mswsock.lib");
		linkerArgs.Add ("psapi.lib");
		linkerArgs.Add ("shell32.lib");
		linkerArgs.Add ("oleaut32.lib");
		linkerArgs.Add ("ole32.lib");
		linkerArgs.Add ("winmm.lib");
		linkerArgs.Add ("user32.lib");
		linkerArgs.Add ("advapi32.lib");

		if (staticLinkCRuntime) {
			// Static release c-runtime support.
			linkerArgs.Add ("libucrt.lib");
			linkerArgs.Add ("libvcruntime.lib");
			linkerArgs.Add ("libcmt.lib");
			linkerArgs.Add ("oldnames.lib");
		} else {
			// Dynamic release c-runtime support.
			linkerArgs.Add ("ucrt.lib");
			linkerArgs.Add ("vcruntime.lib");
			linkerArgs.Add ("msvcrt.lib");
			linkerArgs.Add ("oldnames.lib");
		}

		if (MakeBundle.compress) {
			if (staticLinkMono)
				linkerArgs.Add("zlibstatic.lib");
			else
				linkerArgs.Add("zlib.lib");
		}

		return;
	}

	static void AddGCCSystemLibraries (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime, List<string> linkerArgs)
	{
		if (staticLinkMono) {
			linkerArgs.Add ("-lws2_32");
			linkerArgs.Add ("-lmswsock");
			linkerArgs.Add ("-lpsapi");
			linkerArgs.Add ("-loleaut32");
			linkerArgs.Add ("-lole32");
			linkerArgs.Add ("-lwinmm");
			linkerArgs.Add ("-ladvapi32");
			linkerArgs.Add ("-lversion");
		}

		if (MakeBundle.compress)
			linkerArgs.Add ("-lz");

		return;
	}

	static void AddSystemLibraries (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime, List<string> linkerArgs)
	{
		if (program.IsVSToolChain)
			AddVCSystemLibraries (program, staticLinkMono, staticLinkCRuntime, linkerArgs);
		else
			AddGCCSystemLibraries (program, staticLinkMono, staticLinkCRuntime, linkerArgs);

		return;
	}

	static string GetMonoLibraryName (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime)
	{
		bool vsToolChain = program.IsVSToolChain;
		string monoLibrary = GetEnv ("LIBMONO", "");

		if (monoLibrary.Length == 0) {
			if (staticLinkMono)
				monoLibrary = vsToolChain ? "libmono-static-sgen" : "monosgen-2.0";
			else
				monoLibrary = vsToolChain ? "mono-2.0-sgen" : "monosgen-2.0";
		}

		return monoLibrary;
	}

	static string GetMonoLibraryPath (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime)
	{
		string monoLibraryDir = Path.Combine (GetMonoDir (), "lib");
		string monoLibrary = GetMonoLibraryName (program, staticLinkMono, staticLinkCRuntime);

		if (Path.IsPathRooted (monoLibrary))
			return monoLibrary;

		if (program.IsVSToolChain) {
			if (!monoLibrary.EndsWith (".lib", StringComparison.OrdinalIgnoreCase))
				monoLibrary = monoLibrary + ".lib";
		} else {
			if (!monoLibrary.StartsWith ("lib", StringComparison.OrdinalIgnoreCase))
				monoLibrary = "lib" + monoLibrary;
			if (staticLinkMono) {
				if (!monoLibrary.EndsWith (".dll.a", StringComparison.OrdinalIgnoreCase))
					monoLibrary = monoLibrary + ".dll.a";
			} else {
				if (!monoLibrary.EndsWith (".a", StringComparison.OrdinalIgnoreCase))
					monoLibrary = monoLibrary + ".a";
			}
		}

		return Path.Combine (monoLibraryDir, monoLibrary);
	}

	static void AddMonoLibraries (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime, List<string> linkerArguments)
	{
		bool vsToolChain = program.IsVSToolChain;
		string libPrefix = !vsToolChain ? "-l" : "";
		string libExtension = vsToolChain ? ".lib" : "";
		string monoLibrary = GetMonoLibraryName (program, staticLinkMono, staticLinkCRuntime);

		if (!Path.IsPathRooted (monoLibrary)) {
			if (!monoLibrary.EndsWith (libExtension, StringComparison.OrdinalIgnoreCase))
				monoLibrary = monoLibrary + libExtension;

			linkerArguments.Add (libPrefix + monoLibrary);
		} else {
			linkerArguments.Add (monoLibrary);
		}

		return;
	}

	static void AddVCCompilerArguments (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime, List<string> compilerArgs)
	{
		List<string> includePaths = new List<string> ();
		AddIncludePaths (includePaths);

		if (staticLinkCRuntime)
			// Add targeted c-runtime (MT = static release).
			compilerArgs.Add ("/MT");
		else
			// Add targeted c-runtime (MD = dynamic release).
			compilerArgs.Add ("/MD");

		// Add include search paths.
		foreach (string include in includePaths)
			compilerArgs.Add (String.Format("/I {0}", program.QuoteArg (include)));

		return;
	}

	static void AddGCCCompilerArguments (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime, List<string> compilerArgs)
	{
		List<string> includePaths = new List<string> ();
		AddMonoIncludePaths (includePaths);

		// Add include search paths.
		foreach (string include in includePaths)
			compilerArgs.Add (String.Format ("-I {0}", program.QuoteArg (include)));

		return;
	}

	static void AddCompilerArguments (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime, List<string> compilerArgs)
	{
		if (program.IsVSToolChain)
			AddVCCompilerArguments (program, staticLinkMono, staticLinkCRuntime, compilerArgs);
		else
			AddGCCCompilerArguments (program, staticLinkMono, staticLinkCRuntime, compilerArgs);

		return;
	}

	static void AddVCLinkerArguments (ToolchainProgram linker, bool staticLinkMono, bool staticLinkCRuntime, string customMain, string outputFile, List<string> linkerArgs)
	{
		linkerArgs.Add ("/link");

		var subsystem = GetEnv ("VCSUBSYSTEM", "windows");
		linkerArgs.Add ("/SUBSYSTEM:" + subsystem);

		if (customMain != null && customMain.Length != 0)
			linkerArgs.Add (linker.QuoteArg (customMain));
		else
			linkerArgs.Add ("/ENTRY:mainCRTStartup");

		// Ignore other c-runtime directives from linked libraries.
		linkerArgs.Add ("/NODEFAULTLIB");

		AddMonoLibraries (linker, staticLinkMono, staticLinkCRuntime, linkerArgs);
		AddSystemLibraries (linker, staticLinkMono, staticLinkCRuntime, linkerArgs);

		// Add library search paths.
		List<string> libPaths = new List<string> ();
		AddLibPaths (libPaths);

		foreach (string lib in libPaths)
			linkerArgs.Add (String.Format ("/LIBPATH:{0}", linker.QuoteArg (lib)));

		// Linker output target.
		linkerArgs.Add ("/OUT:" + linker.QuoteArg (outputFile));

		return;
	}

	static void AddGCCLinkerArguments (ToolchainProgram linker, bool staticLinkMono, bool staticLinkCRuntime, string customMain, string outputFile, List<string> linkerArgs)
	{
		// Add library search paths.
		List<string> libPaths = new List<string> ();
		AddMonoLibPaths (libPaths);

		foreach (string lib in libPaths)
			linkerArgs.Add (String.Format ("-L {0}", linker.QuoteArg (lib)));

		// Add libraries.
		if (staticLinkMono)
			linkerArgs.Add ("-Wl,-Bstatic");

		AddMonoLibraries (linker, staticLinkMono, staticLinkCRuntime, linkerArgs);

		if (staticLinkMono)
			linkerArgs.Add ("-Wl,-Bdynamic");

		AddSystemLibraries (linker, staticLinkMono, staticLinkCRuntime, linkerArgs);

		// Linker output target.
		linkerArgs.Add ("-o " + linker.QuoteArg (outputFile));
	}

	static void AddLinkerArguments (ToolchainProgram program, bool staticLinkMono, bool staticLinkCRuntime, string customMain, string outputFile, List<string> linkerArgs)
	{
		if (program.IsVSToolChain)
			AddVCLinkerArguments (program, staticLinkMono, staticLinkCRuntime, customMain, outputFile, linkerArgs);
		else
			AddGCCLinkerArguments (program, staticLinkMono, staticLinkCRuntime, customMain, outputFile, linkerArgs);

		return;
	}

	static void AddVCLibrarianCompilerArguments (ToolchainProgram compiler, string sourceFile, bool staticLinkMono, bool staticLinkCRuntime, List<string> compilerArgs, out string objectFile)
	{
		compilerArgs.Add ("/c");
		compilerArgs.Add (compiler.QuoteArg (sourceFile));

		objectFile = sourceFile + ".obj";
		compilerArgs.Add (String.Format ("/Fo" + compiler.QuoteArg (objectFile)));

		return;
	}

	static void AddGCCLibrarianCompilerArguments (ToolchainProgram compiler, string sourceFile, bool staticLinkMono, bool staticLinkCRuntime, List<string> compilerArgs, out string objectFile)
	{
		compilerArgs.Add ("-c");
		compilerArgs.Add (compiler.QuoteArg (sourceFile));

		objectFile = sourceFile + ".o";
		compilerArgs.Add (String.Format ("-o " + compiler.QuoteArg (objectFile)));

		return;
	}

	static void AddVCLibrarianLinkerArguments (ToolchainProgram librarian, string [] objectFiles, bool staticLinkMono, bool staticLinkCRuntime, string outputFile, List<string> librarianArgs)
	{
		foreach (var objectFile in objectFiles)
			librarianArgs.Add (librarian.QuoteArg (objectFile));

		// Add library search paths.
		List<string> libPaths = new List<string> ();
		AddLibPaths (libPaths);

		foreach (string lib in libPaths) {
			librarianArgs.Add (String.Format ("/LIBPATH:{0}", librarian.QuoteArg (lib)));
		}

		AddMonoLibraries (librarian, staticLinkMono, staticLinkCRuntime, librarianArgs);

		librarianArgs.Add ("/OUT:" + librarian.QuoteArg (output));

		return;
	}

	static void AddGCCLibrarianLinkerArguments (ToolchainProgram librarian, string [] objectFiles, bool staticLinkMono, bool staticLinkCRuntime, string outputFile, List<string> librarianArgs)
	{
		foreach (var objectFile in objectFiles)
			librarianArgs.Add (librarian.QuoteArg (objectFile));

		// Add library search paths.
		List<string> libPaths = new List<string> ();
		AddMonoLibPaths (libPaths);

		foreach (string lib in libPaths)
			librarianArgs.Add (String.Format ("-L {0}", librarian.QuoteArg (lib)));

		AddMonoLibraries (librarian, staticLinkMono, staticLinkCRuntime, librarianArgs);

		librarianArgs.Add ("-o " + librarian.QuoteArg (output));

		return;
	}

	static ToolchainProgram GetAssemblerCompiler ()
	{
		// First check if env is set (old behavior) and use that.
		string assembler = GetEnv ("AS", "");
		if (assembler.Length != 0)
			return new ToolchainProgram ("AS", assembler);

		var vcClangAssembler = VisualStudioSDKToolchainHelper.GetInstance ().GetVCClangCompiler ();
		if (vcClangAssembler == null || vcClangAssembler.Path.Length == 0) {
			// Fallback to GNU assembler if clang for VS was not installed.
			// Why? because mkbundle generates GNU assembler not compilable by VS tools like ml.
			Console.WriteLine (@"Warning: Couldn't find installed Visual Studio SDK (Clang with Microsoft CodeGen), fallback to mingw as.exe and default environment.");
			string asCompiler = Target64BitApplication () ? "x86_64-w64-mingw32-as.exe" : "i686-w64-mingw32-as.exe";
			return new ToolchainProgram (asCompiler, asCompiler);
		}

		return vcClangAssembler;
	}

	static ToolchainProgram GetCCompiler (bool staticLinkMono, bool staticLinkCRuntime)
	{
		ToolchainProgram program = null;

		// First check if env is set (old behavior) and use that.
		string compiler = GetEnv ("CC", "");
		if (compiler.Length != 0) {
			program = new ToolchainProgram ("CC", compiler);
		} else {
			program = VisualStudioSDKToolchainHelper.GetInstance ().GetVCCompiler ();
			if (program == null || program.Path.Length == 0) {
				// Fallback to cl.exe if VC compiler was not installed.
				Console.WriteLine (@"Warning: Couldn't find installed Visual Studio SDK, fallback to cl.exe and default environment.");
				program = new ToolchainProgram ("cl.exe", "cl.exe");
			}
		}

		// Check if we have needed Mono library for targeted toolchain.
		string monoLibraryPath = GetMonoLibraryPath (program, staticLinkMono, staticLinkCRuntime);
		if (!File.Exists (monoLibraryPath) && program.IsVSToolChain) {
			Console.WriteLine (@"Warning: Couldn't find installed matching Mono library: {0}, fallback to mingw gcc.exe and default environment.", monoLibraryPath);
			string gccCompiler = Target64BitApplication () ? "x86_64-w64-mingw32-gcc.exe" : "i686-w64-mingw32-gcc.exe";
			program = new ToolchainProgram (gccCompiler, gccCompiler);
		}

		return program;
	}

	static ToolchainProgram GetLibrarian ()
	{
		ToolchainProgram vcLibrarian = VisualStudioSDKToolchainHelper.GetInstance ().GetVCLibrarian ();
		if (vcLibrarian == null || vcLibrarian.Path.Length == 0) {
			// Fallback to lib.exe if VS was not installed.
			Console.WriteLine (@"Warning: Couldn't find installed Visual Studio SDK, fallback to lib.exe and default environment.");
			return new ToolchainProgram ("lib.exe", "lib.exe");
		}

		return vcLibrarian;
	}

	static string GetCompileAndLinkCommand (ToolchainProgram compiler, string sourceFile, string objectFile, string customMain, bool staticLinkMono, bool staticLinkCRuntime, string outputFile)
	{
		var compilerArgs = new List<string> ();

		AddCompilerArguments (compiler, staticLinkMono, staticLinkCRuntime, compilerArgs);

		// Add source file to compile.
		compilerArgs.Add (compiler.QuoteArg (sourceFile));

		// Add assembled object file.
		compilerArgs.Add (compiler.QuoteArg (objectFile));

		// Add linker arguments.
		AddLinkerArguments (compiler, staticLinkMono, staticLinkCRuntime, customMain, outputFile, compilerArgs);

		return String.Format ("{0} {1}", compiler.QuoteArg (compiler.Path), String.Join (" ", compilerArgs));
	}

	static string GetLibrarianCompilerCommand (ToolchainProgram compiler, string sourceFile, bool staticLinkMono, bool staticLinkCRuntime, out string objectFile)
	{
		var compilerArgs = new List<string> ();

		AddCompilerArguments (compiler, staticLinkMono, staticLinkCRuntime, compilerArgs);

		if (compiler.IsVSToolChain)
			AddVCLibrarianCompilerArguments (compiler, sourceFile, staticLinkMono, staticLinkCRuntime, compilerArgs, out objectFile);
		else
			AddGCCLibrarianCompilerArguments (compiler, sourceFile, staticLinkMono, staticLinkCRuntime, compilerArgs, out objectFile);

		return String.Format ("{0} {1}", compiler.QuoteArg (compiler.Path), String.Join (" ", compilerArgs));
	}

	static string GetLibrarianLinkerCommand (ToolchainProgram librarian, string [] objectFiles, bool staticLinkMono, bool staticLinkCRuntime, string outputFile)
	{
		var librarianArgs = new List<string> ();

		if (librarian.IsVSToolChain)
			AddVCLibrarianLinkerArguments (librarian, objectFiles, staticLinkMono, staticLinkCRuntime, outputFile, librarianArgs);
		else
			AddGCCLibrarianLinkerArguments (librarian, objectFiles, staticLinkMono, staticLinkCRuntime, outputFile, librarianArgs);

		return String.Format ("{0} {1}", librarian.QuoteArg (librarian.Path), String.Join (" ", librarianArgs));
	}
#endregion
}
