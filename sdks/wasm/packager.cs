using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Options;

class Driver {
	static bool enable_debug, enable_linker;
	static string app_prefix, framework_prefix, bcl_prefix, bcl_facades_prefix, out_prefix;
	static HashSet<string> asm_list = new HashSet<string> ();
	static List<string>  file_list = new List<string> ();
	static List<string> assembly_names = new List<string> ();

	const string BINDINGS_ASM_NAME = "WebAssembly.Bindings";
	const string BINDINGS_RUNTIME_CLASS_NAME = "WebAssembly.Runtime";

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
		ReaderParameters rp = new ReaderParameters();
		bool add_pdb = enable_debug && File.Exists (Path.ChangeExtension (ra, "pdb"));
		if (add_pdb) {
			rp.ReadSymbols = true;
		}

		rp.InMemory = true;

		var image = ModuleDefinition.ReadModule (ra, rp);
		if (!asm_list.Add (ra))
			return;
		file_list.Add (ra);
		assembly_names.Add (image.Assembly.Name.Name);
		Debug ($"Processing {ra} debug {add_pdb}");

		if (add_pdb && kind == AssemblyKind.User)
			file_list.Add (Path.ChangeExtension (ra, "pdb"));

		foreach (var ar in image.AssemblyReferences) {
			var resolve = Resolve (ar.Name, out kind);
			Import (resolve, kind);
		}
	}

	void GenDriver (string builddir, List<string> assembly_names, List<string> profilers) {
		var symbols = new List<string> ();
		foreach (var img in assembly_names) {
			symbols.Add (String.Format ("mono_aot_module_{0}_info", img.Replace ('.', '_').Replace ('-', '_')));
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

	void Run (string[] args) {
		var root_assemblies = new List<string> ();
		enable_debug = false;
		var add_binding = true;
		string builddir = null;
		string sdkdir = null;
		string emscripten_sdkdir = null;
		out_prefix = Environment.CurrentDirectory;
		app_prefix = Environment.CurrentDirectory;
		var deploy_prefix = "managed";
		var vfs_prefix = "managed";
		var use_release_runtime = true;
		var enable_aot = false;
		var print_usage = false;
		var emit_ninja = false;
		var runtimeTemplate = "runtime.js";
		var assets = new List<string> ();
		var profilers = new List<string> ();

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

		if (enable_aot)
			enable_linker = true;

		var tool_prefix = Path.GetDirectoryName (typeof (Driver).Assembly.Location);

		//are we working from the tree?
		if (sdkdir != null) {
			framework_prefix = tool_prefix; //all framework assemblies are currently side built to packager.exe
			bcl_prefix = Path.Combine (sdkdir, "wasm-bcl/wasm");
		} else if (Directory.Exists (Path.Combine (tool_prefix, "../out/wasm-bcl/wasm"))) {
			framework_prefix = tool_prefix; //all framework assemblies are currently side built to packager.exe
			bcl_prefix = Path.Combine (tool_prefix, "../out/wasm-bcl/wasm");
			sdkdir = Path.Combine (tool_prefix, "../out");
		} else {
			framework_prefix = Path.Combine (tool_prefix, "framework");
			bcl_prefix = Path.Combine (tool_prefix, "bcl");
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
				Console.WriteLine ($"cp {f} -> {Path.Combine (bcl_dir, Path.GetFileName (f))}");
				File.Copy (f, Path.Combine (bcl_dir, Path.GetFileName (f)));
			}
		}

		if (deploy_prefix.EndsWith ("/"))
			deploy_prefix = deploy_prefix.Substring (0, deploy_prefix.Length - 1);
		if (vfs_prefix.EndsWith ("/"))
			vfs_prefix = vfs_prefix.Substring (0, vfs_prefix.Length - 1);

		var dontlink_assemblies = new Dictionary<string, bool> ();
		dontlink_assemblies [BINDINGS_ASM_NAME] = true;

		var runtime_js = Path.Combine (emit_ninja ? builddir : out_prefix, "runtime.js");
		File.Delete (runtime_js);
		File.Copy (runtimeTemplate, runtime_js);

		var file_list_str = string.Join (",", file_list.Select (f => $"\"{Path.GetFileName (f)}\""));
		var config = String.Format ("config = {{\n \tvfs_prefix: \"{0}\",\n \tdeploy_prefix: \"{1}\",\n \tenable_debugging: {2},\n \tfile_list: [ {3} ],\n", vfs_prefix, deploy_prefix, enable_debug ? "1" : "0", file_list_str);
		if (add_binding || true)
			config += "\tadd_bindings: function() { " + $"Module.mono_bindings_init (\"[{BINDINGS_ASM_NAME}]{BINDINGS_RUNTIME_CLASS_NAME}\");" + " }\n";
		config += "}\n";
		var config_js = Path.Combine (emit_ninja ? builddir : out_prefix, "config.js");
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
				Console.WriteLine ($"Asset: cp {asset} -> {Path.Combine (out_prefix, Path.GetFileName (asset))}");
				File.Copy (asset, 
						Path.Combine (out_prefix, asset));
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
			GenDriver (builddir, assembly_names, profilers);
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
		ninja.WriteLine ("cross = $mono_sdkdir/wasm-cross-release/bin/wasm32-mono-sgen");
		ninja.WriteLine ("emcc = source $emscripten_sdkdir/emsdk_env.sh && emcc");
		// -s ASSERTIONS=2 is very slow
		ninja.WriteLine ("emcc_flags = -Os -g -s DISABLE_EXCEPTION_CATCHING=0 -s ASSERTIONS=1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s \"BINARYEN_TRAP_MODE=\'clamp\'\" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s NO_EXIT_RUNTIME=1 -s ERROR_ON_UNDEFINED_SYMBOLS=1 -s \"EXTRA_EXPORTED_RUNTIME_METHODS=[\'ccall\', \'FS_createPath\', \'FS_createDataFile\', \'cwrap\', \'setValue\', \'getValue\', \'UTF8ToString\']\"");

		// Rules
		ninja.WriteLine ("rule aot");
		ninja.WriteLine ($"  command = MONO_PATH=$mono_path $cross --debug {profiler_aot_args} --aot=llvmonly,asmonly,no-opt,static,direct-icalls,llvm-outfile=$outfile $src_file");
		ninja.WriteLine ("  description = [AOT] $src_file -> $outfile");
		ninja.WriteLine ("rule mkdir");
		ninja.WriteLine ("  command = mkdir -p $out");
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

		ninja.WriteLine ("  command = mono $bcl_dir/monolinker.exe -out $builddir/linker-out -l none --exclude-feature com --exclude-feature remoting $linker_args; for f in $out; do if test ! -f $$f; then echo > empty.cs; csc /out:$$f /target:library empty.cs; fi; done");
		ninja.WriteLine ("  description = [IL-LINK]");

		// Targets
		ninja.WriteLine ("build $appdir: mkdir");
		ninja.WriteLine ("build $appdir/$deploy_prefix: mkdir");
		ninja.WriteLine ("build $appdir/runtime.js: cpifdiff $builddir/runtime.js");
		ninja.WriteLine ("build $appdir/config.js: cpifdiff $builddir/config.js");
		if (enable_aot) {
			var source_file = Path.GetFullPath (Path.Combine (tool_prefix, "driver.c"));
			ninja.WriteLine ($"build $builddir/driver.c: cpifdiff {source_file}");
			ninja.WriteLine ($"build $builddir/driver-gen.c: cpifdiff $builddir/driver-gen.c.in");

			ninja.WriteLine ("build $builddir/driver.o: emcc $builddir/driver.c | $builddir/driver-gen.c");
			ninja.WriteLine ("  flags = -DENABLE_AOT=1");

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
		foreach (var assembly in asm_list) {
			string filename = Path.GetFileName (assembly);
			var filename_noext = Path.GetFileNameWithoutExtension (filename);

			var source_file_path = Path.GetFullPath (assembly);
			string infile = "";

			if (enable_linker) {
				linker_infiles += $" $builddir/linker-in/{filename}";
				linker_ofiles += $" $builddir/linker-out/{filename}";
				infile = $"$builddir/linker-out/{filename}";
				ninja.WriteLine ($"build $builddir/linker-in/{filename}: cpifdiff {source_file_path}");
			} else {
				infile = $"$builddir/{filename}";
				ninja.WriteLine ($"build $builddir/{filename}: cpifdiff {source_file_path}");
			}
			ninja.WriteLine ($"build $appdir/$deploy_prefix/{filename}: cpifdiff {infile}");

			if (enable_aot) {
				string mono_path = enable_linker ? "$builddir/linker-out" : "$builddir";
				string destdir = "$builddir";
				string srcfile = infile;

				string outputs = $"{destdir}/{filename}.bc";
				ninja.WriteLine ($"build {outputs}: aot {srcfile}");
				ninja.WriteLine ($"  src_file={srcfile}");
				ninja.WriteLine ($"  outfile={destdir}/{filename}.bc");
				ninja.WriteLine ($"  mono_path={mono_path}");

				ofiles += " " + ($"{destdir}/{filename}.bc");
			}
		}
		if (enable_aot) {
			ninja.WriteLine ($"build $appdir/mono.js: emcc-link $builddir/driver.o {ofiles} {profiler_libs} $mono_sdkdir/wasm-runtime-release/lib/libmonosgen-2.0.a $mono_sdkdir/wasm-runtime-release/lib/libmono-icall-table.a | $tool_prefix/library_mono.js $tool_prefix/binding_support.js $tool_prefix/dotnet_support.js");
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

}