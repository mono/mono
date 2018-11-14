using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Options;

class Driver {
	static bool enable_debug, enable_linker;
	static string app_prefix, framework_prefix, bcl_prefix, bcl_tools_prefix, bcl_facades_prefix, out_prefix;
	static HashSet<string> asm_map = new HashSet<string> ();
	static List<string>  file_list = new List<string> ();

	const string BINDINGS_ASM_NAME = "WebAssembly.Bindings";
	const string BINDINGS_RUNTIME_CLASS_NAME = "WebAssembly.Runtime";

	class AssemblyData {
		// Assembly name
		public string name;
		// Base filename
		public string filename;
		// Path outside build tree
		public string src_path;
		// Path of .bc file
		public string bc_path;
		// Path in appdir
		public string app_path;
		// Linker input path
		public string linkin_path;
		// Linker output path
		public string linkout_path;
	}

	static List<AssemblyData> assemblies = new List<AssemblyData> ();

	enum AssemblyKind {
		User,
		Framework,
		Bcl,
		None,
	}

	static void Usage () {
		Console.WriteLine ("Usage: packager.exe <options> <assemblies>");
		Console.WriteLine ("Valid options:");
		Console.WriteLine ("\t--help          Show this help message");
		Console.WriteLine ("\t--debug         Enable Debugging (default false)");
		Console.WriteLine ("\t--debugrt       Use the debug runtime (default release) - this has nothing to do with C# debugging");
		Console.WriteLine ("\t--nobinding     Disable binding engine (default include engine)");
		Console.WriteLine ("\t--aot           Enable AOT mode");
		Console.WriteLine ("\t--prefix=x      Set the input assembly prefix to 'x' (default to the current directory)");
		Console.WriteLine ("\t--out=x         Set the output directory to 'x' (default to the current directory)");
		Console.WriteLine ("\t--mono-sdkdir=x Set the mono sdk directory to 'x'");
		Console.WriteLine ("\t--deploy=x      Set the deploy prefix to 'x' (default to 'managed')");
		Console.WriteLine ("\t--vfs=x         Set the VFS prefix to 'x' (default to 'managed')");
		Console.WriteLine ("\t--template=x    Set the template name to  'x' (default to 'runtime.js')");
		Console.WriteLine ("\t--asset=x       Add specified asset 'x' to list of assets to be copied");
		Console.WriteLine ("\t--copy=always|ifnewer        Set the type of copy to perform.");
		Console.WriteLine ("\t\t              'always' overwrites the file if it exists.");
		Console.WriteLine ("\t\t              'ifnewer' copies or overwrites the file if modified or size is different.");
		Console.WriteLine ("\t--profile=x     Enable the 'x' mono profiler.");

		Console.WriteLine ("foo.dll         Include foo.dll as one of the root assemblies");
	}

	static void Debug (string s) {
		Console.WriteLine (s);
	}

	static string FindFrameworkAssembly (string asm) {
		return asm;
	}

	static bool Try (string prefix, string name, out string out_res) {
		out_res = null;

		string res = (Path.Combine (prefix, name));
		if (File.Exists (res)) {
			out_res = Path.GetFullPath (res);
			return true;
		}
		return false;
	}

	static string ResolveWithExtension (string prefix, string name) {
		string res = null;

		if (Try (prefix, name, out res))
			return res;
		if (Try (prefix, name + ".dll", out res))
			return res;
		if (Try (prefix, name + ".exe", out res))
			return res;
		return null;
	}

	static string ResolveUser (string asm_name) {
		return ResolveWithExtension (app_prefix, asm_name);
	}

	static string ResolveFramework (string asm_name) {
		return ResolveWithExtension (framework_prefix, asm_name);
	}

	static string ResolveBcl (string asm_name) {
		return ResolveWithExtension (bcl_prefix, asm_name);
	}

	static string ResolveBclFacade (string asm_name) {
		return ResolveWithExtension (bcl_facades_prefix, asm_name);
	}

	static string Resolve (string asm_name, out AssemblyKind kind) {
		kind = AssemblyKind.User;
		var asm = ResolveUser (asm_name);
		if (asm != null)
			return asm;

		kind = AssemblyKind.Framework;
		asm = ResolveFramework (asm_name);
		if (asm != null)
			return asm;

		kind = AssemblyKind.Bcl;
		asm = ResolveBcl (asm_name);
		if (asm == null)
			asm = ResolveBclFacade (asm_name);
		if (asm != null)
			return asm;

		kind = AssemblyKind.None;
		throw new Exception ($"Could not resolve {asm_name}");
	}

	static void Import (string ra, AssemblyKind kind) {
		if (!asm_map.Add (ra))
			return;
		ReaderParameters rp = new ReaderParameters();
		bool add_pdb = enable_debug && File.Exists (Path.ChangeExtension (ra, "pdb"));
		if (add_pdb) {
			rp.ReadSymbols = true;
		}

		rp.InMemory = true;
		var image = ModuleDefinition.ReadModule (ra, rp);
		file_list.Add (ra);
		Debug ($"Processing {ra} debug {add_pdb}");

		var data = new AssemblyData () { name = image.Assembly.Name.Name, src_path = ra };
		assemblies.Add (data);

		if (add_pdb && kind == AssemblyKind.User)
			file_list.Add (Path.ChangeExtension (ra, "pdb"));

		foreach (var ar in image.AssemblyReferences) {
			var resolve = Resolve (ar.Name, out kind);
			Import (resolve, kind);
		}
	}

	void GenDriver (string builddir, List<string> profilers) {
		var symbols = new List<string> ();
		foreach (var adata in assemblies) {
			symbols.Add (String.Format ("mono_aot_module_{0}_info", adata.name.Replace ('.', '_').Replace ('-', '_')));
		}

		var w = File.CreateText (Path.Combine (builddir, "driver-gen.c.in"));

		foreach (var symbol in symbols) {
			w.WriteLine ($"extern void *{symbol};");
		}

		w.WriteLine ("static void register_aot_modules ()");
		w.WriteLine ("{");
		foreach (var symbol in symbols)
			w.WriteLine ($"\tmono_aot_register_module ({symbol});");
		w.WriteLine ("}");

		foreach (var profiler in profilers) {
			w.WriteLine ($"void mono_profiler_init_{profiler} (const char *desc);");
			w.WriteLine ("EMSCRIPTEN_KEEPALIVE void mono_wasm_load_profiler_" + profiler + " (const char *desc) { mono_profiler_init_" + profiler + " (desc); }");
		}

		w.Close ();
	}

	public static void Main (string[] args) {
		new Driver ().Run (args);
	}

	enum CopyType
	{
		Default,
		Always,
		IfNewer		
	}

	void Run (string[] args) {
		var add_binding = true;
		var root_assemblies = new List<string> ();
		enable_debug = false;
		string builddir = null;
		string sdkdir = null;
		string emscripten_sdkdir = null;
		out_prefix = Environment.CurrentDirectory;
		app_prefix = Environment.CurrentDirectory;
		var deploy_prefix = "managed";
		var vfs_prefix = "managed";
		var use_release_runtime = true;
		var enable_aot = false;
		var enable_dedup = true;
		var print_usage = false;
		var emit_ninja = false;
		var runtimeTemplate = "runtime.js";
		var assets = new List<string> ();
		var profilers = new List<string> ();
		var copyTypeParm = "default";
		var copyType = CopyType.Default;

		var p = new OptionSet () {
				{ "debug", s => enable_debug = true },
				{ "nobinding", s => add_binding = false },
				{ "debugrt", s => use_release_runtime = false },
				{ "out=", s => out_prefix = s },
				{ "appdir=", s => out_prefix = s },
				{ "builddir=", s => builddir = s },
				{ "mono-sdkdir=", s => sdkdir = s },
				{ "emscripten-sdkdir=", s => emscripten_sdkdir = s },
				{ "prefix=", s => app_prefix = s },
				{ "deploy=", s => deploy_prefix = s },
				{ "vfs=", s => vfs_prefix = s },
				{ "aot", s => enable_aot = true },
				{ "template=", s => runtimeTemplate = s },
				{ "asset=", s => assets.Add(s) },
				{ "profile=", s => profilers.Add (s) },
				{ "copy=", s => copyTypeParm = s },
				{ "help", s => print_usage = true },
					};

		var new_args = p.Parse (args).ToArray ();
		foreach (var a in new_args) {
			root_assemblies.Add (a);
		}

		if (print_usage) {
			Usage ();
			return;
		}

		if (!Enum.TryParse(copyTypeParm, true, out copyType)) {
			Console.WriteLine("Invalid copy value");
			Usage ();
			return;
		}

		if (enable_aot)
			enable_linker = true;

		var tool_prefix = Path.GetDirectoryName (typeof (Driver).Assembly.Location);

		//are we working from the tree?
		if (sdkdir != null) {
			framework_prefix = tool_prefix; //all framework assemblies are currently side built to packager.exe
			bcl_prefix = Path.Combine (sdkdir, "wasm-bcl/wasm");
			bcl_tools_prefix = Path.Combine (sdkdir, "wasm-bcl/wasm_tools");
		} else if (Directory.Exists (Path.Combine (tool_prefix, "../out/wasm-bcl/wasm"))) {
			framework_prefix = tool_prefix; //all framework assemblies are currently side built to packager.exe
			bcl_prefix = Path.Combine (tool_prefix, "../out/wasm-bcl/wasm");
			bcl_tools_prefix = Path.Combine (tool_prefix, "../out/wasm-bcl/wasm_tools");
			sdkdir = Path.Combine (tool_prefix, "../out");
		} else {
			framework_prefix = Path.Combine (tool_prefix, "framework");
			bcl_prefix = Path.Combine (tool_prefix, "wasm-bcl/wasm");
			bcl_tools_prefix = Path.Combine (tool_prefix, "wasm-bcl/wasm_tools");
			sdkdir = tool_prefix;
		}
		bcl_facades_prefix = Path.Combine (bcl_prefix, "Facades");

		foreach (var ra in root_assemblies) {
			AssemblyKind kind;
			var resolved = Resolve (ra, out kind);
			Import (resolved, kind);
		}
		if (add_binding)
			Import (ResolveFramework (BINDINGS_ASM_NAME + ".dll"), AssemblyKind.Framework);

		if (builddir != null) {
			emit_ninja = true;
			if (!Directory.Exists (builddir))
				Directory.CreateDirectory (builddir);
		}

		if (!emit_ninja) {
			if (!Directory.Exists (out_prefix))
				Directory.CreateDirectory (out_prefix);
			var bcl_dir = Path.Combine (out_prefix, deploy_prefix);
			if (Directory.Exists (bcl_dir))
				Directory.Delete (bcl_dir, true);
			Directory.CreateDirectory (bcl_dir);
			foreach (var f in file_list) {
				CopyFile(f, Path.Combine (bcl_dir, Path.GetFileName (f)), copyType);
			}
		}

		if (deploy_prefix.EndsWith ("/"))
			deploy_prefix = deploy_prefix.Substring (0, deploy_prefix.Length - 1);
		if (vfs_prefix.EndsWith ("/"))
			vfs_prefix = vfs_prefix.Substring (0, vfs_prefix.Length - 1);

		var dontlink_assemblies = new Dictionary<string, bool> ();
		dontlink_assemblies [BINDINGS_ASM_NAME] = true;

		var runtime_js = Path.Combine (emit_ninja ? builddir : out_prefix, "runtime.js");
		if (emit_ninja) {
			File.Delete (runtime_js);
			File.Copy (runtimeTemplate, runtime_js);
		} else {
			if (File.Exists(runtime_js) && (File.Exists(runtimeTemplate))) {
				CopyFile (runtimeTemplate, runtime_js, CopyType.IfNewer, $"runtime template <{runtimeTemplate}> ");
			} else {
				if (File.Exists(runtimeTemplate))
					CopyFile (runtimeTemplate, runtime_js, CopyType.IfNewer, $"runtime template <{runtimeTemplate}> ");
				else
				{
					var runtime_gen = "\nvar Module = {\n\tonRuntimeInitialized: function () {\n\t\tMONO.mono_load_runtime_and_bcl (\n\t\tconfig.vfs_prefix,\n\t\tconfig.deploy_prefix,\n\t\tconfig.enable_debugging,\n\t\tconfig.file_list,\n\t\tfunction () {\n\t\t\tconfig.add_bindings ();\n\t\t\tApp.init ();\n\t\t}\n\t)\n\t},\n};";
					File.Delete (runtime_js);
					File.WriteAllText (runtime_js, runtime_gen);
				}
			}
		}

		if (!enable_linker || !enable_aot)
			enable_dedup = false;

		AssemblyData dedup_asm = null;

		if (enable_dedup) {
			dedup_asm = new AssemblyData () { name = "aot-dummy",
					filename = "aot-dummy.dll",
					bc_path = "$builddir/aot-dummy.dll.bc",
					app_path = "$appdir/$deploy_prefix/aot-dummy.dll",
					linkout_path = "$builddir/linker-out/aot-dummy.dll"
					};
			assemblies.Add (dedup_asm);
			file_list.Add ("aot-dummy.dll");
		}

		var file_list_str = string.Join (",", file_list.Select (f => $"\"{Path.GetFileName (f)}\"").Distinct());
		var config = String.Format ("config = {{\n \tvfs_prefix: \"{0}\",\n \tdeploy_prefix: \"{1}\",\n \tenable_debugging: {2},\n \tfile_list: [ {3} ],\n", vfs_prefix, deploy_prefix, enable_debug ? "1" : "0", file_list_str);
		if (add_binding || true)
			config += "\tadd_bindings: function() { " + $"Module.mono_bindings_init (\"[{BINDINGS_ASM_NAME}]{BINDINGS_RUNTIME_CLASS_NAME}\");" + " }\n";
		config += "}\n";
		var config_js = Path.Combine (emit_ninja ? builddir : out_prefix, "mono-config.js");
		File.Delete (config_js);
		File.WriteAllText (config_js, config);

		string runtime_dir = Path.Combine (tool_prefix, use_release_runtime ? "release" : "debug");
		if (!emit_ninja) {
			File.Delete (Path.Combine (out_prefix, "mono.js"));
			File.Delete (Path.Combine (out_prefix, "mono.wasm"));

			File.Copy (
					   Path.Combine (runtime_dir, "mono.js"),
					   Path.Combine (out_prefix, "mono.js"));
			File.Copy (
					   Path.Combine (runtime_dir, "mono.wasm"),
					   Path.Combine (out_prefix, "mono.wasm"));

			foreach(var asset in assets)
			{
				CopyFile (asset, 
						Path.Combine (out_prefix, asset), copyType, "Asset: ");
			}
		}

		if (!emit_ninja)
			return;

		if (enable_aot) {
			if (sdkdir == null) {
				Console.WriteLine ("The --mono-sdkdir argument is required when using AOT.");
				Environment.Exit (1);
			}
			if (emscripten_sdkdir == null) {
				Console.WriteLine ("The --emscripten-sdkdir argument is required when using AOT.");
				Environment.Exit (1);
			}
			GenDriver (builddir, profilers);
		}

		string profiler_libs = "";
		string profiler_aot_args = "";
		foreach (var profiler in profilers) {
			profiler_libs += $"$mono_sdkdir/wasm-runtime-release/lib/libmono-profiler-{profiler}-static.a ";
			if (profiler_aot_args != "")
				profiler_aot_args += " ";
			profiler_aot_args += $"--profile={profiler}";
		}

		runtime_dir = Path.GetFullPath (runtime_dir);
		sdkdir = Path.GetFullPath (sdkdir);
		out_prefix = Path.GetFullPath (out_prefix);

		var ninja = File.CreateText (Path.Combine (builddir, "build.ninja"));

		// Defines
		ninja.WriteLine ($"mono_sdkdir = {sdkdir}");
		ninja.WriteLine ($"emscripten_sdkdir = {emscripten_sdkdir}");
		ninja.WriteLine ($"tool_prefix = {tool_prefix}");
		ninja.WriteLine ($"appdir = {out_prefix}");
		ninja.WriteLine ($"builddir = .");
		ninja.WriteLine ($"wasm_runtime_dir = {runtime_dir}");
		ninja.WriteLine ($"deploy_prefix = {deploy_prefix}");
		ninja.WriteLine ($"bcl_dir = {bcl_prefix}");
		ninja.WriteLine ($"tools_dir = {bcl_tools_prefix}");
		ninja.WriteLine ("cross = $mono_sdkdir/wasm-cross-release/bin/wasm32-unknown-none-mono-sgen");
		ninja.WriteLine ("emcc = source $emscripten_sdkdir/emsdk_env.sh && emcc");
		// -s ASSERTIONS=2 is very slow
		ninja.WriteLine ("emcc_flags = -Os -g -s DISABLE_EXCEPTION_CATCHING=0 -s ASSERTIONS=1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s \"BINARYEN_TRAP_MODE=\'clamp\'\" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s NO_EXIT_RUNTIME=1 -s ERROR_ON_UNDEFINED_SYMBOLS=1 -s \"EXTRA_EXPORTED_RUNTIME_METHODS=[\'ccall\', \'cwrap\', \'setValue\', \'getValue\', \'UTF8ToString\']\" -s \"EXPORTED_FUNCTIONS=[\'___cxa_is_pointer_type\', \'___cxa_can_catch\']\"");

		// Rules
		ninja.WriteLine ("rule aot");
		ninja.WriteLine ($"  command = MONO_PATH=$mono_path $cross --debug {profiler_aot_args} --aot=$aot_args,llvmonly,asmonly,no-opt,static,direct-icalls,llvm-outfile=$outfile $src_file");
		ninja.WriteLine ("  description = [AOT] $src_file -> $outfile");
		ninja.WriteLine ("rule aot-instances");
		ninja.WriteLine ($"  command = MONO_PATH=$mono_path $cross --debug {profiler_aot_args} --aot=llvmonly,asmonly,no-opt,static,direct-icalls,llvm-outfile=$outfile,dedup-include=$dedup_image $src_files");
		ninja.WriteLine ("  description = [AOT-INSTANCES] $outfile");
		ninja.WriteLine ("rule mkdir");
		ninja.WriteLine ("  command = mkdir -p $out");
		// Copy $in to $out only if it changed
		ninja.WriteLine ("rule cpifdiff");
		ninja.WriteLine ("  command = if cmp -s $in $out ; then : ; else cp $in $out ; fi");
		ninja.WriteLine ("  restat = true");
		ninja.WriteLine ("rule emcc");
		ninja.WriteLine ("  command = bash -c '$emcc $emcc_flags $flags -c -o $out $in'");
		ninja.WriteLine ("  description = [EMCC] $in -> $out");
		ninja.WriteLine ("rule emcc-link");
		ninja.WriteLine ("  command = bash -c '$emcc $emcc_flags -o $out --js-library $tool_prefix/library_mono.js --js-library $tool_prefix/binding_support.js --js-library $tool_prefix/dotnet_support.js $in'");
		ninja.WriteLine ("  description = [EMCC-LINK] $in -> $out");
		ninja.WriteLine ("rule linker");

		ninja.WriteLine ("  command = mono $tools_dir/monolinker.exe -out $builddir/linker-out -l none --exclude-feature com --exclude-feature remoting $linker_args || exit 1; for f in $out; do if test ! -f $$f; then echo > empty.cs; csc /out:$$f /target:library empty.cs; fi; done");
		ninja.WriteLine ("  description = [IL-LINK]");

		// Targets
		ninja.WriteLine ("build $appdir: mkdir");
		ninja.WriteLine ("build $appdir/$deploy_prefix: mkdir");
		ninja.WriteLine ("build $appdir/runtime.js: cpifdiff $builddir/runtime.js");
		ninja.WriteLine ("build $appdir/mono-config.js: cpifdiff $builddir/mono-config.js");
		if (enable_aot) {
			var source_file = Path.GetFullPath (Path.Combine (tool_prefix, "driver.c"));
			ninja.WriteLine ($"build $builddir/driver.c: cpifdiff {source_file}");
			ninja.WriteLine ($"build $builddir/driver-gen.c: cpifdiff $builddir/driver-gen.c.in");

			ninja.WriteLine ("build $builddir/driver.o: emcc $builddir/driver.c | $builddir/driver-gen.c");
			ninja.WriteLine ("  flags = -DENABLE_AOT=1 -I$mono_sdkdir/wasm-runtime-release/include/mono-2.0");

		} else {
			ninja.WriteLine ("build $appdir/mono.js: cpifdiff $wasm_runtime_dir/mono.js");
			ninja.WriteLine ("build $appdir/mono.wasm: cpifdiff $wasm_runtime_dir/mono.wasm");
		}

		var ofiles = "";
		string linker_infiles = "";
		string linker_ofiles = "";
		if (enable_linker) {
			string path = Path.Combine (builddir, "linker-in");
			if (!Directory.Exists (path))
				Directory.CreateDirectory (path);
		}
		string aot_in_path = enable_linker ? "$builddir/linker-out" : "$builddir";
		foreach (var a in assemblies) {
			var assembly = a.src_path;
			if (assembly == null)
				continue;
			string filename = Path.GetFileName (assembly);
			var filename_noext = Path.GetFileNameWithoutExtension (filename);

			var source_file_path = Path.GetFullPath (assembly);
			string infile = "";

			if (enable_linker) {
				a.linkin_path = $"$builddir/linker-in/{filename}";
				a.linkout_path = $"$builddir/linker-out/{filename}";
				linker_infiles += $" {a.linkin_path}";
				linker_ofiles += $" {a.linkout_path}";
				infile = $"{a.linkout_path}";
				ninja.WriteLine ($"build {a.linkin_path}: cpifdiff {source_file_path}");
			} else {
				infile = $"$builddir/{filename}";
				ninja.WriteLine ($"build $builddir/{filename}: cpifdiff {source_file_path}");
			}
			ninja.WriteLine ($"build $appdir/$deploy_prefix/{filename}: cpifdiff {infile}");

			if (enable_aot) {
				a.bc_path = $"$builddir/{filename}.bc";

				ninja.WriteLine ($"build {a.bc_path}: aot {infile}");
				ninja.WriteLine ($"  src_file={infile}");
				ninja.WriteLine ($"  outfile={a.bc_path}");
				ninja.WriteLine ($"  mono_path={aot_in_path}");
				if (enable_dedup)
					ninja.WriteLine ($"  aot_args=dedup-skip");

				ofiles += " " + ($"{a.bc_path}");
			}
		}
		if (enable_dedup) {
			/*
			 * Run the aot compiler in dedup mode:
			 * mono --aot=<args>,dedup-include=aot-dummy.dll <assemblies> aot-dummy.dll
			 * This will process all assemblies and emit all instances into the aot image of aot-dummy.dll
			 */
			var a = dedup_asm;
			/*
			 * The dedup process will read in the .dedup files created when running with dedup-skip, so add all the
			 * .bc files as dependencies.
			 */
			ninja.WriteLine ($"build {a.bc_path}: aot-instances | {ofiles} {a.linkout_path}");
			ninja.WriteLine ($"  dedup_image={a.filename}");
			ninja.WriteLine ($"  src_files={linker_ofiles} {a.linkout_path}");
			ninja.WriteLine ($"  outfile={a.bc_path}");
			ninja.WriteLine ($"  mono_path={aot_in_path}");
			ninja.WriteLine ($"build {a.app_path}: cpifdiff {a.linkout_path}");
			ofiles += $" {a.bc_path}";
			linker_ofiles += $" {a.linkout_path}";
		}
		if (enable_aot) {
			ninja.WriteLine ($"build $appdir/mono.js: emcc-link $builddir/driver.o {ofiles} {profiler_libs} $mono_sdkdir/wasm-runtime-release/lib/libmonosgen-2.0.a | $tool_prefix/library_mono.js $tool_prefix/binding_support.js $tool_prefix/dotnet_support.js");
		}
		if (enable_linker) {
			string linker_args = "";
			foreach (var assembly in root_assemblies) {
				string filename = Path.GetFileName (assembly);
				linker_args += $"-a linker-in/{filename} ";
			}
			foreach (var assembly in dontlink_assemblies.Keys) {
				linker_args += $"-p copy {assembly} ";
			}
			linker_args += " -d $bcl_dir -c link";
			ninja.WriteLine ("build $builddir/linker-out: mkdir");
			ninja.WriteLine ($"build {linker_ofiles}: linker");
			ninja.WriteLine ($"  linker_args={linker_args}");
		}

		foreach(var asset in assets) {
			var filename = Path.GetFileName (asset);
			var abs_path = Path.GetFullPath (asset);
			ninja.WriteLine ($"build $appdir/{filename}: cpifdiff {abs_path}");
		}

		ninja.Close ();
	}

	static void CopyFile(string sourceFileName, string destFileName, CopyType copyType, string typeFile = "")
	{
		Console.WriteLine($"{typeFile}cp: {copyType} - {sourceFileName} -> {destFileName}");
		switch (copyType)
		{
			case CopyType.Always:
				File.Copy(sourceFileName, destFileName, true);
				break;
			case CopyType.IfNewer:
				if (!File.Exists(destFileName))
				{
					File.Copy(sourceFileName, destFileName);
				}
				else
				{
					var srcInfo = new FileInfo (sourceFileName);
					var dstInfo = new FileInfo (destFileName);
					
					if (srcInfo.LastWriteTime.Ticks > dstInfo.LastWriteTime.Ticks || srcInfo.Length > dstInfo.Length)
						File.Copy(sourceFileName, destFileName, true);
					else
						Console.WriteLine($"    skipping: {sourceFileName}");
				}
				break;
			default:
				File.Copy(sourceFileName, destFileName);
				break;
		}

	}


}