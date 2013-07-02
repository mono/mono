//
// mkbundle: tool to create bundles.
//
// Based on the `make-bundle' Perl script written by Paolo Molaro (lupus@debian.org)
//
// Author:
//   Miguel de Icaza
//
// (C) Novell, Inc 2004
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


#if NET_4_5
using System.Threading.Tasks;
#endif

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
	static bool compress;
	static bool nomain;
	static bool? use_dos2unix = null;
	
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

			case "-c":
				compile_only = true;
				break;
				
			case "-o": 
				if (i+1 == top){
					Help (); 
					return 1;
				}
				output = args [++i];
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
				Console.WriteLine ("Note that statically linking the LGPL Mono runtime has more licensing restrictions than dynamically linking.");
				Console.WriteLine ("See http://www.mono-project.com/Licensing for details on licensing.");
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
			default:
				sources.Add (args [i]);
				break;
			}
			
			if (static_link && style == "windows") {
				Console.Error.WriteLine ("The option `{0}' is not supported on this platform.", args [i]);
				return 1;
			}
		}

		Console.WriteLine ("Sources: {0} Auto-dependencies: {1}", sources.Count, autodeps);
		if (sources.Count == 0 || output == null) {
			Help ();
			Environment.Exit (1);
		}

		List<Assembly> assemblies = LoadAssemblies (sources);
		List<string> files = new List<string> ();
		foreach (Assembly a in assemblies)
			QueueAssembly (files, a.CodeBase);
			
		// Special casing mscorlib.dll: any specified mscorlib.dll cannot be loaded
		// by Assembly.ReflectionFromLoadFrom(). Instead the fx assembly which runs
		// mkbundle.exe is loaded, which is not what we want.
		// So, replace it with whatever actually specified.
		foreach (string srcfile in sources) {
			if (Path.GetFileName (srcfile) == "mscorlib.dll") {
				foreach (string file in files) {
					if (Path.GetFileName (new Uri (file).LocalPath) == "mscorlib.dll") {
						files.Remove (file);
						files.Add (new Uri (Path.GetFullPath (srcfile)).LocalPath);
						break;
					}
				}
				break;
			}
		}

		GenerateBundles (files);
		//GenerateJitWrapper ();
		
		return 0;
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
			byte [] buffer = new byte [8192];

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
				string fname = new Uri (url).LocalPath;
				Stream stream = File.OpenRead (fname);

				long real_size = stream.Length;
				int n;
				if (compress) {
					MemoryStream ms = new MemoryStream ();
					GZipStream deflate = new GZipStream (ms, CompressionMode.Compress, leaveOpen:true);
					while ((n = stream.Read (buffer, 0, buffer.Length)) != 0){
						deflate.Write (buffer, 0, n);
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
			foreach (var url in files) {
				string fname = new Uri (url).LocalPath;
				string aname = Path.GetFileName (fname);
				string encoded = aname.Replace ("-", "_").Replace (".", "_");

				if (prog == null)
					prog = aname;

				var stream = streams [url];
				var real_size = sizes [url];

				Console.WriteLine ("   embedding: " + fname);

				WriteSymbol (ts, "assembly_data_" + encoded, stream.Length);
			
				WriteBuffer (ts, stream, buffer);

				if (compress) {
					tc.WriteLine ("extern const unsigned char assembly_data_{0} [];", encoded);
					tc.WriteLine ("static CompressedAssembly assembly_bundle_{0} = {{{{\"{1}\"," +
								  " assembly_data_{0}, {2}}}, {3}}};",
								  encoded, aname, real_size, stream.Length);
					double ratio = ((double) stream.Length * 100) / real_size;
					Console.WriteLine ("   compression ratio: {0:.00}%", ratio);
				} else {
					tc.WriteLine ("extern const unsigned char assembly_data_{0} [];", encoded);
					tc.WriteLine ("static const MonoBundledAssembly assembly_bundle_{0} = {{\"{1}\", assembly_data_{0}, {2}}};",
								  encoded, aname, real_size);
				}
				stream.Close ();

				c_bundle_names.Add ("assembly_bundle_" + encoded);

				try {
					FileStream cf = File.OpenRead (fname + ".config");
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
				Console.WriteLine ("Machine config from: " + machine_config_file);
				tc.WriteLine ("extern const char machine_config;");
				WriteSymbol (ts, "machine_config", machine_config_file.Length);

				WriteBuffer (ts, conf, buffer);
				ts.Write ("\t.byte 0\n");
				ts.WriteLine ();
			}
			ts.Close ();
			
			Console.WriteLine ("Compiling:");
			string cmd = String.Format ("{0} -o {1} {2} ", GetEnv ("AS", "as"), temp_o, temp_s);
			int ret = Execute (cmd);
			if (ret != 0){
				Error ("[Fail]");
				return;
			}

			if (compress)
				tc.WriteLine ("\nstatic const CompressedAssembly *compressed [] = {");
			else
				tc.WriteLine ("\nstatic const MonoBundledAssembly *bundled [] = {");

			foreach (string c in c_bundle_names){
				tc.WriteLine ("\t&{0},", c);
			}
			tc.WriteLine ("\tNULL\n};\n");
			tc.WriteLine ("static char *image_name = \"{0}\";", prog);

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

			if (compile_only)
				return;

			string zlib = (compress ? "-lz" : "");
			string debugging = "-g";
			string cc = GetEnv ("CC", IsUnix ? "cc" : "gcc -mno-cygwin");

			if (style == "linux")
				debugging = "-ggdb";
			if (static_link) {
				string smonolib;
				if (style == "osx")
					smonolib = "`pkg-config --variable=libdir mono-2`/libmono-2.0.a ";
				else
					smonolib = "-Wl,-Bstatic -lmono-2.0 -Wl,-Bdynamic ";
				cmd = String.Format ("{4} -o {2} -Wall `pkg-config --cflags mono-2` {0} {3} " +
						     "`pkg-config --libs-only-L mono-2` " + smonolib +
						     "`pkg-config --libs-only-l mono-2 | sed -e \"s/\\-lmono-2.0 //\"` {1}",
						     temp_c, temp_o, output, zlib, cc);
			} else {
				
				cmd = String.Format ("{4} " + debugging + " -o {2} -Wall {0} `pkg-config --cflags --libs mono-2` {3} {1}",
						     temp_c, temp_o, output, zlib, cc);
			}
                            
			ret = Execute (cmd);
			if (ret != 0){
				Error ("[Fail]");
				return;
			}
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
	
	static List<Assembly> LoadAssemblies (List<string> sources)
	{
		List<Assembly> assemblies = new List<Assembly> ();
		bool error = false;
		
		foreach (string name in sources){
			Assembly a = LoadAssembly (name);

			if (a == null){
				error = true;
				continue;
			}
			
			assemblies.Add (a);
		}

		if (error)
			Environment.Exit (1);

		return assemblies;
	}
	
	static readonly Universe universe = new Universe ();
	
	static void QueueAssembly (List<string> files, string codebase)
	{
		if (files.Contains (codebase))
			return;

		files.Add (codebase);
		Assembly a = universe.LoadFile (new Uri(codebase).LocalPath);

		if (!autodeps)
			return;
		
		foreach (AssemblyName an in a.GetReferencedAssemblies ()) {
			a = universe.Load (an.Name);
			QueueAssembly (files, a.CodeBase);
		}
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
			Console.WriteLine ("Log: \n" + total_log);
		} catch (IKVM.Reflection.BadImageFormatException f) {
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
		Console.Error.WriteLine (msg);
		Environment.Exit (1);
	}

	static void Help ()
	{
		Console.WriteLine ("Usage is: mkbundle [options] assembly1 [assembly2...]\n\n" +
				   "Options:\n" +
				   "    -c                  Produce stub only, do not compile\n" +
				   "    -o out              Specifies output filename\n" +
				   "    -oo obj             Specifies output filename for helper object file\n" +
				   "    -L path             Adds `path' to the search path for assemblies\n" +
				   "    --nodeps            Turns off automatic dependency embedding (default)\n" +
				   "    --deps              Turns on automatic dependency embedding\n" +
				   "    --keeptemp          Keeps the temporary files\n" +
				   "    --config F          Bundle system config file `F'\n" +
				   "    --config-dir D      Set MONO_CFG_DIR to `D'\n" +
				   "    --machine-config F  Use the given file as the machine.config for the application.\n" +
				   "    --static            Statically link to mono libs\n" +
				   "    --nomain            Don't include a main() function, for libraries\n" +
				   "    -z                  Compress the assemblies before embedding.\n" +
				   "                        You need zlib development headers and libraries.\n");
	}

	[DllImport ("libc")]
	static extern int system (string s);
	[DllImport ("libc")]
	static extern int uname (IntPtr buf);
		
	static void DetectOS ()
	{
		if (!IsUnix) {
			Console.WriteLine ("OS is: Windows");
			style = "windows";
			return;
		}

		IntPtr buf = Marshal.AllocHGlobal (8192);
		if (uname (buf) != 0){
			Console.WriteLine ("Warning: Unable to detect OS");
			Marshal.FreeHGlobal (buf);
			return;
		}
		string os = Marshal.PtrToStringAnsi (buf);
		Console.WriteLine ("OS is: " + os);
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

	static int Execute (string cmdLine)
	{
		if (IsUnix) {
			Console.WriteLine (cmdLine);
			return system (cmdLine);
		}
		
		// on Windows, we have to pipe the output of a
		// `cmd` interpolation to dos2unix, because the shell does not
		// strip the CRLFs generated by the native pkg-config distributed
		// with Mono.
		//
		// But if it's *not* on cygwin, just skip it.
		
		// check if dos2unix is applicable.
		if (use_dos2unix == null) {
			use_dos2unix = false;
			try {
				var dos2unix = Process.Start ("dos2unix");
				dos2unix.StandardInput.WriteLine ("aaa");
				dos2unix.StandardInput.WriteLine ("\u0004");
				dos2unix.WaitForExit ();
				if (dos2unix.ExitCode == 0)
					use_dos2unix = true;
			} catch {
				// ignore
			}
		}
		// and if there is no dos2unix, just run cmd /c.
		if (use_dos2unix == false) {
			Console.WriteLine (cmdLine);
			ProcessStartInfo dos2unix = new ProcessStartInfo ();
			dos2unix.UseShellExecute = false;
			dos2unix.FileName = "cmd";
			dos2unix.Arguments = String.Format ("/c \"{0}\"", cmdLine);

			using (Process p = Process.Start (dos2unix)) {
				p.WaitForExit ();
				return p.ExitCode;
			}
		}

		StringBuilder b = new StringBuilder ();
		int count = 0;
		for (int i = 0; i < cmdLine.Length; i++) {
			if (cmdLine [i] == '`') {
				if (count % 2 != 0) {
					b.Append ("|dos2unix");
				}
				count++;
			}
			b.Append (cmdLine [i]);
		}
		cmdLine = b.ToString ();
		Console.WriteLine (cmdLine);
			
		ProcessStartInfo psi = new ProcessStartInfo ();
		psi.UseShellExecute = false;
		psi.FileName = "sh";
		psi.Arguments = String.Format ("-c \"{0}\"", cmdLine);

		using (Process p = Process.Start (psi)) {
			p.WaitForExit ();
			return p.ExitCode;
		}
	}

	static string GetEnv (string name, string defaultValue) 
	{
		string s = Environment.GetEnvironmentVariable (name);
		return s != null ? s : defaultValue;
	}
}
