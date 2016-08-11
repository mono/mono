//
// mkbundle: tool to create bundles.
//
// Based on the `make-bundle' Perl script written by Paolo Molaro (lupus@debian.org)
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
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

class MakeBundle {
	static string output = "a.out";
	static string object_out = null;
	static List<string> link_paths = new List<string> ();
	static bool autodeps = false;
	static bool keeptemp = false;
	static bool compile_only = false;
	static bool static_link = false;
	static string config_file = null;
	static string machine_config_file = null;
	static string config_dir = null;
	static string style = "linux";
	static string os_message = "";
	static bool compress;
	static bool nomain;
	static bool? use_dos2unix = null;
	static bool skip_scan;
	static string ctor_func;
	static bool quiet;
	static string cross_target = null;
	static string fetch_target = null;
	static bool custom_mode = true;
	static string embedded_options = null;
	static string runtime = null;
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
				custom_mode = false;
				autodeps = true;
				cross_target = args [++i];
				break;

			case "--fetch-target":
				if (i+1 == top){
					Help (); 
					return 1;
				}
				fetch_target = args [++i];
				break;

			case "--list-targets":
				var wc = new WebClient ();
				var s = wc.DownloadString (new Uri (target_server + "target-list.txt"));
				Console.WriteLine ("Cross-compilation targets available:\n" + s);
				
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
			case "--runtime":
				if (i+1 == top){
					Help (); 
					return 1;
				}
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
				if (!quiet) {
					Console.WriteLine ("Note that statically linking the LGPL Mono runtime has more licensing restrictions than dynamically linking.");
					Console.WriteLine ("See http://www.mono-project.com/Licensing for details on licensing.");
				}
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
					Console.Error.WriteLine ("Invalid style '{0}' - only 'windows', 'mac' and 'linux' are supported for --style argument", style);
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
			default:
				sources.Add (args [i]);
				break;
			}

		}

		if (fetch_target != null){
			var truntime = Path.Combine (targets_dir, fetch_target, "mono");
			Directory.CreateDirectory (Path.GetDirectoryName (truntime));
			var wc = new WebClient ();
			var uri = new Uri ($"{target_server}{fetch_target}");
			try {
				if (!quiet){
					Console.WriteLine ($"Downloading runtime {uri} to {truntime}");
				}
				
				wc.DownloadFile (uri, truntime);
			} catch {
				Console.Error.WriteLine ($"Failure to download the specified runtime from {uri}");
				File.Delete (truntime);
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
		List<string> files = new List<string> ();
		foreach (string file in assemblies)
			if (!QueueAssembly (files, file))
				return 1;

		if (custom_mode)
			GenerateBundles (files);
		else {
			if (cross_target == "default")
				runtime = null;
			else {
				if (runtime == null){
					if (cross_target == null){
						Console.Error.WriteLine ("you should specify either a --runtime or a --cross compilation target");
						Environment.Exit (1);
					}
					runtime = Path.Combine (targets_dir, cross_target, "mono");
					if (!File.Exists (runtime)){
						Console.Error.WriteLine ($"The runtime for the {cross_target} does not exist, use --fetch-target {cross_target} to download first");
						return 1;
					}
				} else {
					if (!File.Exists (runtime)){
						Console.Error.WriteLine ($"The Mono runtime specified with --runtime does not exist");
						return 1;
					}
				}
				
				Console.WriteLine ("Using runtime {0}", runtime);
			}
			GeneratePackage (files);
		}
		
		return 0;
	}

	static string targets_dir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".mono", "targets");
	
	static void CommandLocalTargets ()
	{
		string [] targets;

		Console.WriteLine ("Available targets:");
		Console.WriteLine ("\tdefault\t- Current System Mono");
		try {
			targets = Directory.GetDirectories (targets_dir);
		} catch {
			return;
		}
		foreach (var target in targets){
			var p = Path.Combine (target, "mono");
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

		public void Dump ()
		{
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
			Console.Error.WriteLine ("The file {0} does not exist", file);
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
				Console.Error.WriteLine ("You must specify at least one runtime with --runtime or --cross");
				Environment.Exit (1);
			}
		}
		if (!File.Exists (runtime)){
			Console.Error.WriteLine ($"The specified runtime at {runtime} does not exist");
			Environment.Exit (1);
		}
		
		if (ctor_func != null){
			Console.Error.WriteLine ("--static-ctor not supported with package bundling, you must use native compilation for this");
			return false;
		}
		
		var maker = new PackageMaker (output);
		maker.AddFile (runtime);
		
		foreach (var url in files){
			string fname = LocateFile (new Uri (url).LocalPath);
			string aname = Path.GetFileName (fname);

			maker.Add ("assembly:" + aname, fname);
			if (File.Exists (fname + ".config"))
				maker.Add ("config:" + aname, fname + ".config");
		}
		if (!MaybeAddFile (maker, "systemconfig:", config_file) || !MaybeAddFile (maker, "machineconfig:", machine_config_file))
			return false;

		if (config_dir != null)
			maker.Add ("config_dir:", config_dir);
		if (embedded_options != null)
			maker.AddString ("options:", embedded_options);
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

#if XAMARIN_ANDROID
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
#else
			tc.WriteLine ("#include <mono/metadata/mono-config.h>");
			tc.WriteLine ("#include <mono/metadata/assembly.h>\n");
#endif

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
				string aname = Path.GetFileName (fname);
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
					Error (String.Format ("Failure to open {0}", config_file));
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
					Error (String.Format ("Failure to open {0}", machine_config_file));
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

			if (compress)
				tc.WriteLine ("\nstatic const CompressedAssembly *compressed [] = {");
			else
				tc.WriteLine ("\nstatic const MonoBundledAssembly *bundled [] = {");

			foreach (string c in c_bundle_names){
				tc.WriteLine ("\t&{0},", c);
			}
			tc.WriteLine ("\tNULL\n};\n");
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

			if (!nomain) {
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
				string vsPath = GetEnv("VSINCLUDE", @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC");
				string monoPath = GetEnv("MONOPREFIX", @"C:\Program Files (x86)\Mono");

				string[] includes = new string[] {winsdkPath + @"\Include\um", winsdkPath + @"\Include\shared", vsPath + @"\include", monoPath + @"\include\mono-2.0"};
				string[] libs = new string[] { winsdkPath + @"\Lib\winv6.3\um\x86" , vsPath + @"\lib" };
				string monoFile;

				var compilerArgs = new List<string>();
				foreach (string include in includes)
					compilerArgs.Add(String.Format ("/I {0}", quote (include)));

				if (static_link)
					monoFile = LocateFile (monoPath + @"\lib\monosgen-2.0.lib");
				else
					monoFile = LocateFile (monoPath + @"\lib\monosgen-2.0.dll");

				compilerArgs.Add("/MD");
				compilerArgs.Add(temp_c);
				compilerArgs.Add(temp_o);
				compilerArgs.Add("/link");

				if (nomain)
					compilerArgs.Add("/NOENTRY");
					compilerArgs.Add("/DLL");

				foreach (string lib in libs)
					compilerArgs.Add(String.Format ("/LIBPATH:{0}", quote(lib)));
				compilerArgs.Add (quote(monoFile));

				string cl_cmd = String.Format("{0} {1}", compiler, String.Join(" ", compilerArgs.ToArray()));
				Execute (cl_cmd);
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
					string smonolib;
					if (style == "osx")
						smonolib = "`pkg-config --variable=libdir mono-2`/libmono-2.0.a ";
					else
						smonolib = "-Wl,-Bstatic -lmono-2.0 -Wl,-Bdynamic ";
					cmd = String.Format("{4} -o '{2}' -Wall `pkg-config --cflags mono-2` {0} {3} " +
						"`pkg-config --libs-only-L mono-2` " + smonolib +
						"`pkg-config --libs-only-l mono-2 | sed -e \"s/\\-lmono-2.0 //\"` {1}",
						temp_c, temp_o, output, zlib, cc);
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
				Assembly a = LoadAssembly (name);

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

		if (error)
			Environment.Exit (1);

		return assemblies;
	}
	
	static readonly Universe universe = new Universe ();
	static readonly Dictionary<string, string> loaded_assemblies = new Dictionary<string, string> ();
	
	static bool QueueAssembly (List<string> files, string codebase)
	{
		// Console.WriteLine ("CODE BASE IS {0}", codebase);
		if (files.Contains (codebase))
			return true;

		var path = new Uri(codebase).LocalPath;
		var name = Path.GetFileName (path);
		string found;
		if (loaded_assemblies.TryGetValue (name, out found)) {
			Error (string.Format ("Duplicate assembly name `{0}'. Both `{1}' and `{2}' use same assembly name.", name, path, found));
			return false;
		}

		loaded_assemblies.Add (name, path);

		files.Add (codebase);
		if (!autodeps)
			return true;
		try {
			Assembly a = universe.LoadFile (path);

			foreach (AssemblyName an in a.GetReferencedAssemblies ()) {
				a = universe.Load (an.FullName);
				if (!QueueAssembly (files, a.CodeBase))
					return false;
			}
		} catch (Exception) {
			if (!skip_scan)
				throw;
		}

		return true;
	}

	static Assembly LoadAssembly (string assembly)
	{
		Assembly a;
		
		try {
			char[] path_chars = { '/', '\\' };
			
			if (assembly.IndexOfAny (path_chars) != -1) {
				a = universe.LoadFile (assembly);
			} else {
				string ass = assembly;
				if (ass.EndsWith (".dll"))
					ass = assembly.Substring (0, assembly.Length - 4);
				a = universe.Load (ass);
			}
			return a;
		} catch (FileNotFoundException){
			string total_log = "";
			
			foreach (string dir in link_paths){
				string full_path = Path.Combine (dir, assembly);
				if (!assembly.EndsWith (".dll") && !assembly.EndsWith (".exe"))
					full_path += ".dll";
				
				try {
					a = universe.LoadFile (full_path);
					return a;
				} catch (FileNotFoundException ff) {
					total_log += ff.FusionLog;
					continue;
				}
			}
			Error ("Cannot find assembly `" + assembly + "'" );
			if (!quiet)
				Console.WriteLine ("Log: \n" + total_log);
		} catch (IKVM.Reflection.BadImageFormatException f) {
			if (skip_scan)
				throw;
			Error ("Cannot load assembly (bad file format) " + f.Message);
		} catch (FileLoadException f){
			Error ("Cannot load assembly " + f.Message);
		} catch (ArgumentNullException){
			Error("Cannot load assembly (null argument)");
		}
		return null;
	}

	static void Error (string msg)
	{
		Console.Error.WriteLine ("ERROR: " + msg);
		Environment.Exit (1);
	}

	static void Help ()
	{
		Console.WriteLine ("Usage is: mkbundle [options] assembly1 [assembly2...]\n\n" +
				   "Options:\n" +
				   "    --config F          Bundle system config file `F'\n" +
				   "    --config-dir D      Set MONO_CFG_DIR to `D'\n" +
				   "    --deps              Turns on automatic dependency embedding (default on simple)\n" +
				   "    -L path             Adds `path' to the search path for assemblies\n" +
				   "    --machine-config F  Use the given file as the machine.config for the application.\n" +
				   "    -o out              Specifies output filename\n" +
				   "    --nodeps            Turns off automatic dependency embedding (default on custom)\n" +
				   "    --skip-scan         Skip scanning assemblies that could not be loaded (but still embed them).\n" +
				   "\n" + 
				   "--simple   Simple mode does not require a C toolchain and can cross compile\n" + 
				   "    --cross TARGET      Generates a binary for the given TARGET\n"+
				   "    --local-targets     Lists locally available targets\n" +
				   "    --list-targets      Lists available targets on the remote server\n" +
				   "    --options OPTIONS   Embed the specified Mono command line options on target\n" +
				   "    --runtime RUNTIME   Manually specifies the Mono runtime to use\n" +
				   "    --target-server URL Specified a server to download targets from, default is " + target_server + "\n" +
				   "\n" +
				   "--custom   Builds a custom launcher, options for --custom\n" +
				   "    -c                  Produce stub only, do not compile\n" +
				   "    -oo obj             Specifies output filename for helper object file\n" +
				   "    --dos2unix[=true|false]\n" +
				   "                        When no value provided, or when `true` specified\n" +
				   "                        `dos2unix` will be invoked to convert paths on Windows.\n" +
				   "                        When `--dos2unix=false` used, dos2unix is NEVER used.\n" +
				   "    --keeptemp          Keeps the temporary files\n" +
				   "    --static            Statically link to mono libs\n" +
				   "    --nomain            Don't include a main() function, for libraries\n" +
				   "    -z                  Compress the assemblies before embedding.\n" +
				   "    --static-ctor ctor  Add a constructor call to the supplied function.\n" +
				   "                        You need zlib development headers and libraries.\n");
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
				Error (String.Format("[Fail] {0}", ret));
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
