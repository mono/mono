using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Options;

class Driver {
	static bool enable_debug;
	static string app_prefix, framework_prefix, bcl_prefix, bcl_facades_prefix, out_prefix;
	static HashSet<string> asm_list = new HashSet<string> ();
	static List<string>  file_list = new List<string> ();
	static List<string> assembly_names = new List<string> ();

	const string BINDINGS_ASM_NAME = "bindings";
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

	void GenDriver (string builddir, List<string> assembly_names) {
		var symbols = new List<string> ();
		foreach (var img in assembly_names) {
			symbols.Add (String.Format ("mono_aot_module_{0}_info", img.Replace ('.', '_').Replace ('-', '_')));
		}

		var w = File.CreateText (Path.Combine (builddir, "driver-gen.c"));

		foreach (var symbol in symbols) {
			w.WriteLine ($"extern void *{symbol};");
		}

		w.WriteLine ("static void register_aot_modules ()");
		w.WriteLine ("{");
		foreach (var symbol in symbols)
			w.WriteLine ($"\tmono_aot_register_module ({symbol});");
		w.WriteLine ("}");

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

		var tool_prefix = Path.GetDirectoryName (typeof (Driver).Assembly.Location);

		//are we working from the tree?
		if (sdkdir != null) {
			framework_prefix = tool_prefix; //all framework assemblies are currently side built to packager.exe
			bcl_prefix = Path.Combine (sdkdir, "bcl/wasm");
		} else if (Directory.Exists (Path.Combine (tool_prefix, "../out/bcl/wasm"))) {
			framework_prefix = tool_prefix; //all framework assemblies are currently side built to packager.exe
			bcl_prefix = Path.Combine (tool_prefix, "../out/bcl/wasm");
		} else {
			framework_prefix = Path.Combine (tool_prefix, "framework");
			bcl_prefix = Path.Combine (tool_prefix, "bcl");
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

		var template = File.ReadAllText (Path.Combine (tool_prefix, "runtime.g.js"));
		
		var file_list_str = string.Join (",", file_list.Select (f => $"\"{Path.GetFileName (f)}\""));
		template = template.Replace ("@FILE_LIST@", file_list_str);
		template = template.Replace ("@VFS_PREFIX@", vfs_prefix);
		template = template.Replace ("@DEPLOY_PREFIX@", deploy_prefix);
		template = template.Replace ("@ENABLE_DEBUGGING@", enable_debug ? "1" : "0");
		if (add_binding)
			template = template.Replace ("@BINDINGS_LOADING@", $"Module.mono_bindings_init (\"[{BINDINGS_ASM_NAME}]{BINDINGS_RUNTIME_CLASS_NAME}\");");
		else
			template = template.Replace ("@BINDINGS_LOADING@", "");

		var runtime_js = Path.Combine (emit_ninja ? builddir : out_prefix, "runtime.js");
		Debug ($"create {runtime_js}");
		File.Delete (runtime_js);
		File.WriteAllText (runtime_js, template);

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
		}

		GenDriver (builddir, assembly_names);

		File.Delete (Path.Combine (builddir, "driver.c"));
		File.Copy (Path.Combine (tool_prefix, "driver.c"), Path.Combine (builddir, "driver.c"));

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
		ninja.WriteLine ("cross = $mono_sdkdir/wasm-cross/bin/wasm32-mono-sgen");
		ninja.WriteLine ("emcc = source $emscripten_sdkdir/emsdk_env.sh && emcc");
		// -s ASSERTIONS=2 is very slow
		ninja.WriteLine ("emcc_flags = -Os -g -s ASSERTIONS=1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s \"BINARYEN_TRAP_MODE='clamp'\" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s NO_EXIT_RUNTIME=1 -s \"EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']\"");

		// Rules
		ninja.WriteLine ("rule aot");
		ninja.WriteLine ($"  command = MONO_PATH=$mono_path $cross --debug --aot=llvmonly,asmonly,no-opt,static,llvm-outfile=$outfile $src_file");
		ninja.WriteLine ("  description = [AOT] $src_file -> $outfile");
		ninja.WriteLine ("rule mkdir");
		ninja.WriteLine ("  command = mkdir -p $out");
		ninja.WriteLine ("rule cpifdiff");
		ninja.WriteLine ("  command = if cmp -s $in $out ; then : ; else cp $in $out ; fi");
		ninja.WriteLine ("  restat = true");
		ninja.WriteLine ("rule emcc");
		ninja.WriteLine ("  command = $emcc $emcc_flags $flags -c -o $out $in");
		ninja.WriteLine ("  description = [EMCC] $in -> $out");
		ninja.WriteLine ("rule emcc-link");
		ninja.WriteLine ("  command = $emcc $emcc_flags -o $out --js-library $tool_prefix/library_mono.js --js-library $tool_prefix/binding_support.js --js-library $tool_prefix/dotnet_support.js $in");
		ninja.WriteLine ("  description = [EMCC-LINK] $in -> $out");

		// Targets
		ninja.WriteLine ("build $appdir: mkdir");
		ninja.WriteLine ("build $appdir/$deploy_prefix: mkdir");
		ninja.WriteLine ("build $appdir/runtime.js: cpifdiff $builddir/runtime.js");
		if (!enable_aot)
			ninja.WriteLine ("build $appdir/mono.js: cpifdiff $wasm_runtime_dir/mono.js");
		ninja.WriteLine ("build $appdir/mono.wasm: cpifdiff $wasm_runtime_dir/mono.wasm");
		ninja.WriteLine ("build $builddir/driver.o: emcc $builddir/driver.c");
		if (enable_aot)
			ninja.WriteLine ("  flags = -DENABLE_AOT=1");

		var ofiles = "";
		foreach (var assembly in asm_list) {
			string filename = Path.GetFileName (assembly);
			var filename_noext = Path.GetFileNameWithoutExtension (filename);

			File.Copy (assembly, Path.Combine (builddir, filename), true);
			ninja.WriteLine ($"build $appdir/$deploy_prefix/{filename}: cpifdiff $builddir/{filename}");

			if (enable_aot) {
				string destdir = null;
				string srcfile = null;
				destdir = "$builddir";
				srcfile = $"{filename}";

				string outputs = $"{destdir}/{filename}.bc";
				ninja.WriteLine ($"build {outputs}: aot {srcfile}");
				ninja.WriteLine ($"  src_file={srcfile}");
				ninja.WriteLine ($"  outfile={destdir}/{filename}.bc");
				ninja.WriteLine ($"  mono_path={destdir}");

				ofiles += " " + ($"{destdir}/{filename}.bc");
			}
		}
		if (enable_aot) {
			ninja.WriteLine ($"build $appdir/mono.js: emcc-link $builddir/driver.o $mono_sdkdir/wasm-runtime/lib/libmonosgen-2.0.a {ofiles} | $tool_prefix/library_mono.js $tool_prefix/binding_support.js $tool_prefix/dotnet_support.js");
		}

		ninja.Close ();
	}

}