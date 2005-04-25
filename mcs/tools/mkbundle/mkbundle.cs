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
using System.Xml;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Posix;

class MakeBundle {
	static string output = "a.out";
	static string object_out = null;
	static ArrayList link_paths = new ArrayList ();
	static bool autodeps = false;
	static bool keeptemp = false;
	static bool compile_only = false;
	static bool static_link = false;
	static string config_file = null;
	
	static int Main (string [] args)
	{
		ArrayList sources = new ArrayList ();
		int top = args.Length;
		link_paths.Add (".");
		
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
				break;
			case "--config":
				if (i+1 == top){
					Help ();
					return 1;
				}
				config_file = args [++i];
				break;
			default:
				sources.Add (args [i]);
				break;
			}
		}

		Console.WriteLine ("Sources: {0} Auto-dependencies: {1}", sources.Count, autodeps);
		if (sources.Count == 0 || output == null) {
			Help ();
			Environment.Exit (1);
		}

		ArrayList assemblies = LoadAssemblies (sources);
		ArrayList files = new ArrayList ();
		foreach (Assembly a in assemblies)
			QueueAssembly (files, a.CodeBase);

		GenerateBundles (files);
		//GenerateJitWrapper ();
		
		return 0;
	}

	static void WriteSymbol (StreamWriter sw, string name, long size)
	{
		sw.WriteLine (
	        	".globl {0}\n" +
			"\t.section .rodata\n" +
			"\t.align 32\n" +
			"\t.type {0}, @object\n" +
			"\t.size {0}, {1}\n" +
			"{0}:\n",
			name, size);

	}
	
	static void GenerateBundles (ArrayList files)
	{
		string temp_s = "temp.s"; // Path.GetTempFileName ();
		string temp_c = "temp.c";
		string temp_o = Path.GetTempFileName () + ".o";

		if (compile_only)
			temp_c = output;
		if (object_out != null)
			temp_o = object_out;
		
		try {
			ArrayList c_bundle_names = new ArrayList ();
			ArrayList config_names = new ArrayList ();
			byte [] buffer = new byte [8192];

			StreamWriter ts = new StreamWriter (File.Create (temp_s));
			StreamWriter tc = new StreamWriter (File.Create (temp_c));
			string prog = null;

			tc.WriteLine ("/* This source code was produced by mkbundle, do not edit */");
			tc.WriteLine ("#include <mono/metadata/assembly.h>\n");
			foreach (string url in files){
				string fname = url.Substring (7);
				string aname = fname.Substring (fname.LastIndexOf ("/") + 1);
				string encoded = aname.Replace ("-", "_").Replace (".", "_");

				if (prog == null)
					prog = aname;
				
				Console.WriteLine ("   embedding: " + fname);
				
				FileInfo fi = new FileInfo (fname);
				FileStream fs = File.OpenRead (fname);

				WriteSymbol (ts, "assembly_data_" + encoded, fi.Length);

				int n;
				while ((n = fs.Read (buffer, 0, 8192)) != 0){
					for (int i = 0; i < n; i++){
						ts.Write ("\t.byte {0}\n", buffer [i]);
					}
				}
				ts.WriteLine ();

				tc.WriteLine ("extern const unsigned char assembly_data_{0} [];", encoded);
				tc.WriteLine ("static const MonoBundledAssembly assembly_bundle_{0} = {{\"{1}\", assembly_data_{0}, {2}}};",
					      encoded, aname, fi.Length);

				c_bundle_names.Add ("assembly_bundle_" + encoded);

				try {
					FileStream cf = File.OpenRead (fname + ".config");
					Console.WriteLine (" config from: " + fname + ".config");
					tc.WriteLine ("extern const unsigned char assembly_config_{0} [];", encoded);
					WriteSymbol (ts, "assembly_config_" + encoded, cf.Length);
					while ((n = cf.Read (buffer, 0, 8192)) != 0){
						for (int i = 0; i < n; i++){
							ts.Write ("\t.byte {0}\n", buffer [i]);
						}
					}
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
				tc.WriteLine ("extern const unsigned char system_config;");
				WriteSymbol (ts, "system_config", config_file.Length);

				int n;
				while ((n = conf.Read (buffer, 0, 8192)) != 0){
					for (int i = 0; i < n; i++){
						ts.Write ("\t.byte {0}\n", buffer [i]);
					}
				}
				// null terminator 
				ts.Write ("\t.byte 0\n");
				ts.WriteLine ();
			}
			ts.Close ();
			
			Console.WriteLine ("Compiling:");
			string cmd = String.Format ("as -o {0} {1} ", temp_o, temp_s);
			Console.WriteLine (cmd);
			int ret = system (cmd);
			if (ret != 0){
				Error ("[Fail]");
				return;
			}

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
			tc.WriteLine ("}\n");
				      
			StreamReader s = new StreamReader (Assembly.GetAssembly (typeof(MakeBundle)).GetManifestResourceStream ("template.c"));
			tc.Write (s.ReadToEnd ());
			tc.Close ();

			if (compile_only)
				return;

			if (static_link)
				cmd = String.Format ("cc -o {2} -Wall `pkg-config --cflags mono` {0} " +
						     "`pkg-config --libs-only-L mono` -Wl,-Bstatic " +
						     "`pkg-config --libs-only-l mono | sed -e \"s/\\-lm //\" | " +
						     "sed -e \"s/\\-ldl //\" | sed -e \"s/\\-lpthread //\"` " +
						     "-Wl,-Bdynamic -ldl -lm -lrt {1}",
						     temp_c, temp_o, output);
			else
				cmd = String.Format ("cc -o {2} -Wall {0} `pkg-config --cflags --libs mono` {1}",
						     temp_c, temp_o, output);

                            
			Console.WriteLine (cmd);
			ret = system (cmd);
			if (ret != 0){
				Error ("[Fail]");
				return;
			}
			Console.WriteLine ("Done");
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
	
	static ArrayList LoadAssemblies (ArrayList sources)
	{
		ArrayList assemblies = new ArrayList ();
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
	
	static void QueueAssembly (ArrayList files, string codebase)
	{
		if (files.Contains (codebase))
			return;

		files.Add (codebase);
		Assembly a = Assembly.LoadFrom (codebase);

		if (!autodeps)
			return;
		
		foreach (AssemblyName an in a.GetReferencedAssemblies ()) {
			a = Assembly.Load (an);
			QueueAssembly (files, a.CodeBase);
		}
	}

	static Assembly LoadAssembly (string assembly)
	{
		Assembly a;
		
		try {
			char[] path_chars = { '/', '\\' };
			
			if (assembly.IndexOfAny (path_chars) != -1) {
				a = Assembly.LoadFrom (assembly);
			} else {
				string ass = assembly;
				if (ass.EndsWith (".dll"))
					ass = assembly.Substring (0, assembly.Length - 4);
				a = Assembly.Load (ass);
			}
			return a;
		} catch (FileNotFoundException){
			string total_log = "";
			
			foreach (string dir in link_paths){
				string full_path = Path.Combine (dir, assembly);
				if (!assembly.EndsWith (".dll") && !assembly.EndsWith (".exe"))
					full_path += ".dll";
				
				try {
					a = Assembly.LoadFrom (full_path);
					return a;
				} catch (FileNotFoundException ff) {
					total_log += ff.FusionLog;
					continue;
				}
			}
			Error ("Cannot find assembly `" + assembly + "'" );
			Console.WriteLine ("Log: \n" + total_log);
		} catch (BadImageFormatException f) {
			Error ("Cannot load assembly (bad file format)" + f.FusionLog);
		} catch (FileLoadException f){
			Error ("Cannot load assembly " + f.FusionLog);
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
				   "    -c          Produce stub only, do not compile\n" +
				   "    -o out      Specifies output filename\n" +
				   "    -oo obj     Specifies output filename for helper object file" +
				   "    -L path     Adds `path' to the search path for assemblies\n" +
				   "    --nodeps    Turns off automatic dependency embedding (default)\n" +
				   "    --deps      Turns on automatic dependency embedding\n" +
				   "    --keeptemp  Keeps the temporary files\n" +
				   "    --config F  Bundle system config file `F'\n" +
				   "    --static    Statically link to mono libs\n");
	}

	[DllImport ("libc")]
	static extern int system (string s);
}

