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
	static bool keeptemp = false;
	static bool compile_only = false;
	static bool static_link = false;
	static string config_file = null;
	static string machine_config_file = null;
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
	static string aot_mode = "";
	static string aot_runtime = null;
	static int? aot_dedup_assembly = null;
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
					Error ("You can not specify one of --runtime, --sdk or --cross");
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
					Error ("You can not specify one of --runtime, --sdk or --cross");
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
			case "--aot-runtime":
				aot_runtime = args [++i];
				break;
			case "--aot-dedup":
				if (i+1 == top) {
					Console.WriteLine ("Usage: --aot-dedup <container_dll> ");
					return 1;
				}
				var dedup_file = args [++i];
				sources.Add (dedup_file);
				aot_dedup_assembly = sources.Count () - 1;
				break;
			case "--aot-mode":
				if (i+1 == top) {
					Console.WriteLine ("Need string of aot mode (full, llvmonly). Omit for normal AOT.");
					return 1;
				}

				aot_mode = args [++i];

				if (aot_mode != "full" && aot_mode != "llvmonly") {
					Console.WriteLine ("Need string of aot mode (full, llvmonly). Omit for normal AOT.");
					return 1;
				}

				aot_compile = true;
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
				break;
			default:
				sources.Add (args [i]);
				break;
			}
		}
		// Modern bundling starts here
		if (!custom_mode){
			if (runtime != null){
				// Nothing to do here, the user has chosen to manually specify --runtime nad libraries
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
		if (!File.Exists (runtime))
			Error ($"The SDK location does not contain a {path}/bin/mono runtime");
		lib_path = Path.Combine (path, "lib", "mono", "4.5");
		if (!Directory.Exists (lib_path))
			Error ($"The SDK location does not contain a {path}/lib/mono/4.5 directory");
		link_paths.Add (lib_path);
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
			sw.WriteLine (
				".globl _{0}\n" +
				"\t.section .rdata,\"dr\"\n" +
				"\t.align 32\n" +
				"_{0}:\n",
				name, size);
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
		const int align = 4096;
		Stream package;
		
		public PackageMaker (string output)
		{
			package = File.Create (output, 128*1024);
			if (IsUnix){
				File.SetAttributes (output, unchecked ((FileAttributes) 0x80000000));
			}
		}

		public int AddFile (string fname)
		{
			using (Stream fileStream = File.OpenRead (fname)){
				var ret = fileStream.Length;

				if (!quiet)
					Console.WriteLine ("At {0:x} with input {1}", package.Position, fileStream.Length);
				fileStream.CopyTo (package);
				package.Position = package.Position + (align - (package.Position % align));
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
			var bytes = Encoding.UTF8.GetBytes (text);
			locations [entry] = Tuple.Create (package.Position, bytes.Length);
			package.Write (bytes, 0, bytes.Length);
			package.Position = package.Position + (align - (package.Position % align));
		}

		public void AddStringPair (string entry, string key, string value)
		{
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
		
		var maker = new PackageMaker (output);
		Console.WriteLine ("Using runtime: " + runtime);
		maker.AddFile (runtime);
		
		foreach (var url in files){
			string fname = LocateFile (new Uri (url).LocalPath);
			string aname = MakeBundle.GetAssemblyName (fname);

			maker.Add ("assembly:" + aname, fname);
			Console.WriteLine ("     Assembly: " + fname);
			if (File.Exists (fname + ".config")){
				maker.Add ("config:" + aname, fname + ".config");
				Console.WriteLine ("       Config: " + runtime);
			}
		}
		
		if (!MaybeAddFile (maker, "systemconfig:", config_file) || !MaybeAddFile (maker, "machineconfig:", machine_config_file))
			return false;

		if (config_dir != null)
			maker.Add ("config_dir:", config_dir);
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
		string temp_o = "temp.o";

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
void          mono_register_bundled_assemblies (const MonoBundledAssembly **assemblies);
void          mono_register_config_for_assembly (const char* assembly_name, const char* config_xml);
");
			} else {
				tc.WriteLine ("#include <mono/metadata/mono-config.h>");
				tc.WriteLine ("#include <mono/metadata/assembly.h>\n");
				tc.WriteLine ("#include <mono/jit/jit.h>\n");
			}

			if (compress) {
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
					tc.WriteLine ("extern const unsigned char assembly_config_{0} [];", encoded);
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


			// AOT baked in plus loader
			foreach (string asm in aot_names){
				tc.WriteLine ("\textern const void *mono_aot_module_{0}_info;", asm);
			}

			tc.WriteLine ("\nstatic void install_aot_modules (void) {\n");
			foreach (string asm in aot_names){
				tc.WriteLine ("\tmono_aot_register_module (mono_aot_module_{0}_info);\n", asm);
			}

			string enum_aot_mode;
			switch (aot_mode) {
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
			tc.WriteLine ("\tmono_jit_set_aot_mode ({0});", enum_aot_mode);

			tc.WriteLine ("\n}\n");


			tc.WriteLine ("static char *image_name = \"{0}\";", prog);

			if (ctor_func != null) {
				tc.WriteLine ("\nextern void {0} (void);", ctor_func);
				tc.WriteLine ("\n__attribute__ ((constructor)) static void mono_mkbundle_ctor (void)");
				tc.WriteLine ("{{\n\t{0} ();\n}}", ctor_func);
			}

			tc.WriteLine ("\nstatic void install_dll_config_files (void) {\n");
			foreach (string[] ass in config_names){
				tc.WriteLine ("\tmono_register_config_for_assembly (\"{0}\", assembly_config_{1});\n", ass [0], ass [1]);
			}
			if (config_file != null)
				tc.WriteLine ("\tmono_config_parse_memory (&system_config);\n");
			if (machine_config_file != null)
				tc.WriteLine ("\tmono_register_machine_config (&machine_config);\n");
			tc.WriteLine ("}\n");

			if (config_dir != null)
				tc.WriteLine ("static const char *config_dir = \"{0}\";", config_dir);
			else
				tc.WriteLine ("static const char *config_dir = NULL;");

			Stream template_stream;
			if (compress) {
				template_stream = System.Reflection.Assembly.GetAssembly (typeof(MakeBundle)).GetManifestResourceStream ("template_z.c");
			} else {
				template_stream = System.Reflection.Assembly.GetAssembly (typeof(MakeBundle)).GetManifestResourceStream ("template.c");
			}

			StreamReader s = new StreamReader (template_stream);
			string template = s.ReadToEnd ();
			tc.Write (template);

			if (!nomain && custom_main == null) {
				Stream template_main_stream = System.Reflection.Assembly.GetAssembly (typeof(MakeBundle)).GetManifestResourceStream ("template_main.c");
				StreamReader st = new StreamReader (template_main_stream);
				string maintemplate = st.ReadToEnd ();
				tc.Write (maintemplate);
			}

			tc.Close ();

			string assembler = GetEnv("AS", "as");
			string as_cmd = String.Format("{0} -o {1} {2} ", assembler, temp_o, temp_s);
			Execute(as_cmd);

			if (compile_only)
				return;

			if (!quiet)
				Console.WriteLine("Compiling:");

			if (style == "windows")
			{

				Func<string, string> quote = (pp) => { return "\"" + pp + "\""; };

				string compiler = GetEnv("CC", "cl.exe");
				string winsdkPath = GetEnv("WINSDK", @"C:\Program Files (x86)\Windows Kits\8.1");
				string vsPath = GetEnv("VSINCLUDE", @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC");
				string monoPath = GetEnv("MONOPREFIX", @"C:\Program Files (x86)\Mono");

				string[] includes = new string[] {winsdkPath + @"\Include\um", winsdkPath + @"\Include\shared", vsPath + @"\include", monoPath + @"\include\mono-2.0", "." };
				// string[] libs = new string[] { winsdkPath + @"\Lib\winv6.3\um\x86" , vsPath + @"\lib" };
				var linkLibraries = new string[] {  "kernel32.lib",
												"version.lib",
												"Ws2_32.lib",
												"Mswsock.lib",
												"Psapi.lib",
												"shell32.lib",
												"OleAut32.lib",
												"ole32.lib",
												"winmm.lib",
												"user32.lib",
												"libvcruntime.lib",
												"advapi32.lib",
												"OLDNAMES.lib",
												"libucrt.lib" };

				string glue_obj = "mkbundle_glue.obj";
				string monoLib;

				if (static_link)
					monoLib = LocateFile (monoPath + @"\lib\monosgen-2.0-static.lib");

				else {
					Console.WriteLine ("WARNING: Dynamically linking the Mono runtime on Windows is not a tested option.");
					monoLib = LocateFile (monoPath + @"\lib\monosgen-2.0.lib");
					LocateFile (monoPath + @"\lib\monosgen-2.0.dll"); // in this case, the .lib is just the import library, and the .dll is also needed
				}

				var compilerArgs = new List<string>();
				compilerArgs.Add("/MT");

				foreach (string include in includes)
					compilerArgs.Add(String.Format ("/I {0}", quote (include)));

				if (!nomain || custom_main != null) {
					compilerArgs.Add(quote(temp_c));
					compilerArgs.Add(quote(temp_o));
					if (custom_main != null)
						compilerArgs.Add(quote(custom_main));
					compilerArgs.Add(quote(monoLib));
					compilerArgs.Add("/link");
					compilerArgs.Add("/NODEFAULTLIB");
					compilerArgs.Add("/SUBSYSTEM:windows");
					compilerArgs.Add("/ENTRY:mainCRTStartup");
					compilerArgs.AddRange(linkLibraries);
					compilerArgs.Add("/out:"+ output);

					string cl_cmd = String.Format("{0} {1}", compiler, String.Join(" ", compilerArgs.ToArray()));
					Execute (cl_cmd);
				}
				else
				{
					// we are just creating a .lib
					compilerArgs.Add("/c"); // compile only
					compilerArgs.Add(temp_c);
					compilerArgs.Add(String.Format("/Fo" + glue_obj)); // .obj output name

					string cl_cmd = String.Format("{0} {1}", compiler, String.Join(" ", compilerArgs.ToArray()));
					Execute (cl_cmd);

					string librarian = GetEnv ("LIB", "lib.exe");
					var librarianArgs = new List<string> ();
					librarianArgs.Add (String.Format ("/out:{0}.lib" + output));
					librarianArgs.Add (temp_o);
					librarianArgs.Add (glue_obj);
					librarianArgs.Add (monoLib);
					string lib_cmd = String.Format("{0} {1}", librarian, String.Join(" ", librarianArgs.ToArray()));
					Execute (lib_cmd);
				}
			}
			else
			{
				string zlib = (compress ? "-lz" : "");
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

					cmd = String.Format("{4} -o '{2}' -Wall `pkg-config --cflags mono-2` {0} {3} " +
						"`pkg-config --libs-only-L mono-2` {5} {6} " + platform_libs +
						"`pkg-config --libs-only-l mono-2 | sed -e \"s/\\-lmono-2.0 //\"` {1} -g ",
						temp_c, temp_o, output, zlib, cc, smonolib, String.Join (" ", aot_paths));
				}
				else
				{

					cmd = String.Format("{4} " + debugging + " -o '{2}' -Wall {0} `pkg-config --cflags --libs mono-2` {3} {1}",
						temp_c, temp_o, output, zlib, cc);
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

			foreach (AssemblyName an in a.GetReferencedAssemblies ()) {
				a = LoadAssembly (an.Name);
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

		var aot_mode_string = "";
		if (aot_mode != null)
			aot_mode_string = "," + aot_mode;

		var dedup_mode_string = "";
		StringBuilder all_assemblies = null;
		if (aot_dedup_assembly != null) {
			dedup_mode_string = ",dedup-skip";
			all_assemblies = new StringBuilder("");
		}

		for (int i=0; i < files.Count; i++) {
			var fileName = files [i];
			string path = LocateFile (new Uri (fileName).LocalPath);
			string outPath = String.Format ("{0}.aot_out", path);
			aot_paths.Add (outPath);
			var name = System.Reflection.Assembly.LoadFrom(path).GetName().Name;
			aot_names.Add (EncodeAotSymbol (name));

			if (aot_dedup_assembly != null && i != (int) aot_dedup_assembly) {
				all_assemblies.Append (path);
				all_assemblies.Append (" ");
				Execute (String.Format ("{0} --aot={1},outfile={2}{3}{4} {5}",
					aot_runtime, aot_args, outPath, aot_mode_string, dedup_mode_string, path));
			}
		}
		if (aot_dedup_assembly != null) {
			string fileName = files [(int) aot_dedup_assembly];
			string path = LocateFile (new Uri (fileName).LocalPath);
			dedup_mode_string = String.Format (",dedup-include={0}", path);
			string outPath = String.Format ("{0}.aot_out", path);
			Execute (String.Format ("{0} --aot={1},outfile={2}{3}{4} {5} {6}",
				aot_runtime, aot_args, outPath, aot_mode_string, dedup_mode_string, path, all_assemblies.ToString ()));
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
}
