using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Options;
using Mono.Cecil.Cil;

//
// Google V8 style options:
// - bool: --foo/--no-foo
//

enum FlagType {
	BoolFlag,
}

// 'Option' is already used by Mono.Options
class Flag {
	public Flag (string name, string desc, FlagType type) {
		Name = name;
		FlagType = type;
		Description = desc;
	}

	public string Name {
		get; set;
	}

	public FlagType FlagType {
		get; set;
	}

	public string Description {
		get; set;
	}
}

class BoolFlag : Flag {
	public BoolFlag (string name, string description, bool def_value, Action<bool> action) : base (name, description, FlagType.BoolFlag) {
		Setter = action;
		DefaultValue = def_value;
	}

	public Action<bool> Setter {
		get; set;
	}

	public bool DefaultValue {
		get; set;
	}
}

class Driver {
	static bool enable_debug, enable_linker;
	static string app_prefix, framework_prefix, bcl_tools_prefix, bcl_facades_prefix, out_prefix;
	static List<string> bcl_prefixes;
	static HashSet<string> asm_map = new HashSet<string> ();
	static List<string>  file_list = new List<string> ();
	static HashSet<string> assemblies_with_dbg_info = new HashSet<string> ();
	static List<string> root_search_paths = new List<string>();

	const string BINDINGS_ASM_NAME = "WebAssembly.Bindings";
	const string BINDINGS_RUNTIME_CLASS_NAME = "WebAssembly.Runtime";
	const string HTTP_ASM_NAME = "WebAssembly.Net.Http";
	const string WEBSOCKETS_ASM_NAME = "WebAssembly.Net.WebSockets";
	const string BINDINGS_MODULE = "corebindings.o";
	const string BINDINGS_MODULE_SUPPORT = "$tool_prefix/src/binding_support.js";

	class AssemblyData {
		// Assembly name
		public string name;
		// Base filename
		public string filename;
		// Path outside build tree
		public string src_path;
		// Path of .bc file
		public string bc_path;
		// Path of the wasm object file
		public string o_path;
		// Path in appdir
		public string app_path;
		// Path of the AOT depfile
		public string aot_depfile_path;
		// Linker input path
		public string linkin_path;
		// Linker output path
		public string linkout_path;
		// AOT input path
		public string aotin_path;
		// Final output path after IL strip
		public string final_path;
		// Whenever to AOT this assembly
		public bool aot;
	}

	static List<AssemblyData> assemblies = new List<AssemblyData> ();

	enum AssemblyKind {
		User,
		Framework,
		Bcl,
		None,
	}

	void AddFlag (OptionSet options, Flag flag) {
		if (flag is BoolFlag) {
			options.Add (flag.Name, s => (flag as BoolFlag).Setter (true));
			options.Add ("no-" + flag.Name, s => (flag as BoolFlag).Setter (false));
		}
		option_list.Add (flag);
	}

	static List<Flag> option_list = new List<Flag> ();

	static void Usage () {
		Console.WriteLine ("Usage: packager.exe <options> <assemblies>");
		Console.WriteLine ("Valid options:");
		Console.WriteLine ("\t--help          Show this help message");
		Console.WriteLine ("\t--debugrt       Use the debug runtime (default release) - this has nothing to do with C# debugging");
		Console.WriteLine ("\t--aot           Enable AOT mode");
		Console.WriteLine ("\t--aot-interp    Enable AOT+INTERP mode");
		Console.WriteLine ("\t--prefix=x      Set the input assembly prefix to 'x' (default to the current directory)");
		Console.WriteLine ("\t--out=x         Set the output directory to 'x' (default to the current directory)");
		Console.WriteLine ("\t--mono-sdkdir=x Set the mono sdk directory to 'x'");
		Console.WriteLine ("\t--deploy=x      Set the deploy prefix to 'x' (default to 'managed')");
		Console.WriteLine ("\t--vfs=x         Set the VFS prefix to 'x' (default to 'managed')");
		Console.WriteLine ("\t--template=x    Set the template name to  'x' (default to 'runtime.js')");
		Console.WriteLine ("\t--asset=x       Add specified asset 'x' to list of assets to be copied");
		Console.WriteLine ("\t--search-path=x Add specified path 'x' to list of paths used to resolve assemblies");
		Console.WriteLine ("\t--copy=always|ifnewer        Set the type of copy to perform.");
		Console.WriteLine ("\t\t              'always' overwrites the file if it exists.");
		Console.WriteLine ("\t\t              'ifnewer' copies or overwrites the file if modified or size is different.");
		Console.WriteLine ("\t--profile=x     Enable the 'x' mono profiler.");
		Console.WriteLine ("\t--aot-assemblies=x List of assemblies to AOT in AOT+INTERP mode.");
		Console.WriteLine ("\t--aot-profile=x Use 'x' as the AOT profile.");
		Console.WriteLine ("\t--link-mode=sdkonly|all        Set the link type used for AOT. (EXPERIMENTAL)");
		Console.WriteLine ("\t--pinvoke-libs=x DllImport libraries used.");
		Console.WriteLine ("\t\t              'sdkonly' only link the Core libraries.");
		Console.WriteLine ("\t\t              'all' link Core and User assemblies. (default)");

		Console.WriteLine ("foo.dll         Include foo.dll as one of the root assemblies");
		Console.WriteLine ();

		Console.WriteLine ("Additional options (--option/--no-option):");
		foreach (var flag in option_list) {
			if (flag is BoolFlag) {
				Console.WriteLine ("  --" + flag.Name + " (" + flag.Description + ")");
				Console.WriteLine ("        type: bool  default: " + ((flag as BoolFlag).DefaultValue ? "true" : "false"));
			}
		}
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
		foreach (var prefix in bcl_prefixes) {
			string res = ResolveWithExtension (prefix, asm_name);
			if (res != null)
				return res;
		}
		return null;
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

	static bool is_sdk_assembly (string filename) {
		foreach (var prefix in bcl_prefixes)
			if (filename.StartsWith (prefix))
				return true;
		return false;
	}

	static void Import (string ra, AssemblyKind kind) {
		if (!asm_map.Add (ra))
			return;
		ReaderParameters rp = new ReaderParameters();
		bool add_pdb = enable_debug && File.Exists (Path.ChangeExtension (ra, "pdb"));
		if (add_pdb) {
			rp.ReadSymbols = true;
			// Facades do not have symbols
			rp.ThrowIfSymbolsAreNotMatching = false;
			rp.SymbolReaderProvider = new DefaultSymbolReaderProvider(false);
		}

		var resolver = new DefaultAssemblyResolver();
		root_search_paths.ForEach(resolver.AddSearchDirectory);
		foreach (var prefix in bcl_prefixes)
			resolver.AddSearchDirectory (prefix);
		resolver.AddSearchDirectory(bcl_facades_prefix);
		resolver.AddSearchDirectory(framework_prefix);		
		rp.AssemblyResolver = resolver;

		rp.InMemory = true;
		var image = ModuleDefinition.ReadModule (ra, rp);
		file_list.Add (ra);
		//Debug ($"Processing {ra} debug {add_pdb}");

		var data = new AssemblyData () { name = image.Assembly.Name.Name, src_path = ra };
		assemblies.Add (data);

		if (add_pdb && (kind == AssemblyKind.User || kind == AssemblyKind.Framework)) {
			file_list.Add (Path.ChangeExtension (ra, "pdb"));
			assemblies_with_dbg_info.Add (Path.ChangeExtension (ra, "pdb"));
		}

		var parent_kind = kind;

		foreach (var ar in image.AssemblyReferences) {
			// Resolve using root search paths first
			AssemblyDefinition resolved = null;
			try {
				resolved = image.AssemblyResolver.Resolve(ar, rp);
			} catch {
			}

			if (resolved == null && is_sdk_assembly (ra))
				// FIXME: netcore assemblies have missing references
				continue;

			if (resolved != null) {
				Import (resolved.MainModule.FileName, parent_kind);
			} else {
				var resolve = Resolve (ar.Name, out kind);
				Import(resolve, kind);
			}
		}
	}

	void GenDriver (string builddir, List<string> profilers, ExecMode ee_mode, bool link_icalls) {
		var symbols = new List<string> ();
		foreach (var adata in assemblies) {
			if (adata.aot)
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

		switch (ee_mode) {
		case ExecMode.AotInterp:
			w.WriteLine ("#define EE_MODE_LLVMONLY_INTERP 1");
			break;
		case ExecMode.Aot:
			w.WriteLine ("#define EE_MODE_LLVMONLY 1");
			break;
		default:
			break;
		}

		if (link_icalls)
			w.WriteLine ("#define LINK_ICALLS 1");

		w.Close ();
	}

	public static int Main (string[] args) {
		return new Driver ().Run (args);
	}

	enum CopyType
	{
		Default,
		Always,
		IfNewer		
	}

	enum ExecMode {
		Interp = 1,
		Aot = 2,
		AotInterp = 3
	}

	enum LinkMode
	{
		SdkOnly,
		All		
	}

	class WasmOptions {
		public bool Debug;
		public bool DebugRuntime;
		public bool AddBinding;
		public bool Linker;
		public bool LinkIcalls;
		public bool ILStrip;
		public bool LinkerVerbose;
		public bool EnableZLib;
		public bool EnableThreads;
		public bool NativeStrip;
	}

	int Run (string[] args) {
		var add_binding = true;
		var root_assemblies = new List<string> ();
		enable_debug = false;
		string builddir = null;
		string sdkdir = null;
		string emscripten_sdkdir = null;
		var aot_assemblies = "";
		out_prefix = Environment.CurrentDirectory;
		app_prefix = Environment.CurrentDirectory;
		var deploy_prefix = "managed";
		var vfs_prefix = "managed";
		var use_release_runtime = true;
		var enable_aot = false;
		var enable_dedup = true;
		var print_usage = false;
		var emit_ninja = false;
		bool build_wasm = false;
		bool enable_lto = false;
		bool link_icalls = false;
		bool gen_pinvoke = false;
		bool enable_zlib = false;
		bool enable_threads = false;
		bool is_netcore = false;
		var il_strip = false;
		var linker_verbose = false;
		var runtimeTemplate = "runtime.js";
		var assets = new List<string> ();
		var profilers = new List<string> ();
		var pinvoke_libs = "";
		var copyTypeParm = "default";
		var copyType = CopyType.Default;
		var ee_mode = ExecMode.Interp;
		var linkModeParm = "all";
		var linkMode = LinkMode.All;
		var linkDescriptor = "";
		var framework = "";
		var netcore_sdkdir = "";
		string coremode, usermode;
		string aot_profile = null;

		var opts = new WasmOptions () {
				AddBinding = true,
				Debug = false,
				DebugRuntime = false,
				Linker = false,
				ILStrip = true,
				LinkerVerbose = false,
				EnableZLib = false,
				NativeStrip = true
			};

		var p = new OptionSet () {
				{ "nobinding", s => opts.AddBinding = false },
				{ "out=", s => out_prefix = s },
				{ "appdir=", s => out_prefix = s },
				{ "builddir=", s => builddir = s },
				{ "mono-sdkdir=", s => sdkdir = s },
				{ "emscripten-sdkdir=", s => emscripten_sdkdir = s },
				{ "netcore-sdkdir=", s => netcore_sdkdir = s },
				{ "prefix=", s => app_prefix = s },
				{ "deploy=", s => deploy_prefix = s },
				{ "vfs=", s => vfs_prefix = s },
				{ "aot", s => ee_mode = ExecMode.Aot },
				{ "aot-interp", s => ee_mode = ExecMode.AotInterp },
				{ "template=", s => runtimeTemplate = s },
				{ "asset=", s => assets.Add(s) },
				{ "search-path=", s => root_search_paths.Add(s) },
				{ "profile=", s => profilers.Add (s) },
				{ "copy=", s => copyTypeParm = s },
				{ "aot-assemblies=", s => aot_assemblies = s },
				{ "aot-profile=", s => aot_profile = s },
				{ "link-mode=", s => linkModeParm = s },
				{ "link-descriptor=", s => linkDescriptor = s },
				{ "pinvoke-libs=", s => pinvoke_libs = s },
				{ "framework=", s => framework = s },
				{ "help", s => print_usage = true },
					};

		AddFlag (p, new BoolFlag ("debug", "enable c# debugging", opts.Debug, b => opts.Debug = b));
		AddFlag (p, new BoolFlag ("debugrt", "enable debug runtime", opts.DebugRuntime, b => opts.DebugRuntime = b));
		AddFlag (p, new BoolFlag ("linker", "enable the linker", opts.Linker, b => opts.Linker = b));
		AddFlag (p, new BoolFlag ("binding", "enable the binding engine", opts.AddBinding, b => opts.AddBinding = b));
		AddFlag (p, new BoolFlag ("link-icalls", "link away unused icalls", opts.LinkIcalls, b => opts.LinkIcalls = b));
		AddFlag (p, new BoolFlag ("il-strip", "strip IL code from assemblies in AOT mode", opts.ILStrip, b => opts.ILStrip = b));
		AddFlag (p, new BoolFlag ("linker-verbose", "set verbose option on linker", opts.LinkerVerbose, b => opts.LinkerVerbose = b));
		AddFlag (p, new BoolFlag ("zlib", "enable the use of zlib for System.IO.Compression support", opts.EnableZLib, b => opts.EnableZLib = b));
		AddFlag (p, new BoolFlag ("threads", "enable threads", opts.EnableThreads, b => opts.EnableThreads = b));
		AddFlag (p, new BoolFlag ("native-strip", "strip final executable", opts.NativeStrip, b => opts.NativeStrip = b));

		var new_args = p.Parse (args).ToArray ();
		foreach (var a in new_args) {
			root_assemblies.Add (a);
		}

		if (print_usage) {
			Usage ();
			return 0;
		}

		if (!Enum.TryParse(copyTypeParm, true, out copyType)) {
			Console.WriteLine("Invalid copy value");
			Usage ();
			return 1;
		}

		if (!Enum.TryParse(linkModeParm, true, out linkMode)) {
			Console.WriteLine("Invalid link-mode value");
			Usage ();
			return 1;
		}

		enable_debug = opts.Debug;
		enable_linker = opts.Linker;
		add_binding = opts.AddBinding;
		use_release_runtime = !opts.DebugRuntime;
		il_strip = opts.ILStrip;
		linker_verbose = opts.LinkerVerbose;
		gen_pinvoke = pinvoke_libs != "";
		enable_zlib = opts.EnableZLib;
		enable_threads = opts.EnableThreads;

		if (ee_mode == ExecMode.Aot || ee_mode == ExecMode.AotInterp)
			enable_aot = true;

		if (enable_aot || opts.Linker)
			enable_linker = true;
		if (opts.LinkIcalls)
			link_icalls = true;
		if (!enable_linker || !enable_aot)
			enable_dedup = false;
		if (enable_aot || link_icalls || gen_pinvoke || profilers.Count > 0)
			build_wasm = true;
		if (!enable_aot && link_icalls)
			enable_lto = true;
		if (ee_mode != ExecMode.Aot)
			// Can't strip out IL code in mixed mode, since the interpreter might execute some methods even if they have AOTed code available
			il_strip = false;

		if (aot_assemblies != "") {
			if (ee_mode != ExecMode.AotInterp) {
				Console.Error.WriteLine ("The --aot-assemblies= argument requires --aot-interp.");
				return 1;
			}
		}
		if (link_icalls && !enable_linker) {
			Console.Error.WriteLine ("The --link-icalls option requires the --linker option.");
			return 1;
		}
		if (framework != "") {
			if (framework.StartsWith ("netcoreapp")) {
				is_netcore = true;
				if (netcore_sdkdir == "") {
					Console.Error.WriteLine ("The --netcore-sdkdir= argument is required.");
					return 1;
				}
			} else {
				Console.Error.WriteLine ("The only valid value for --framework is 'netcoreapp...'");
				return 1;
			}
		}

		if (aot_profile != null && !File.Exists (aot_profile)) {
			Console.Error.WriteLine ($"AOT profile file '{aot_profile}' not found.");
			return 1;
		}

		var tool_prefix = Path.GetDirectoryName (typeof (Driver).Assembly.Location);

		//are we working from the tree?
		if (sdkdir != null) {
			framework_prefix = Path.Combine (tool_prefix, "framework"); //all framework assemblies are currently side built to packager.exe
		} else if (Directory.Exists (Path.Combine (tool_prefix, "../out/wasm-bcl/wasm"))) {
			framework_prefix = Path.Combine (tool_prefix, "framework"); //all framework assemblies are currently side built to packager.exe
			sdkdir = Path.Combine (tool_prefix, "../out");
		} else {
			framework_prefix = Path.Combine (tool_prefix, "framework");
			sdkdir = tool_prefix;
		}
		string bcl_root = Path.Combine (sdkdir, "wasm-bcl");
		var bcl_prefix = Path.Combine (bcl_root, "wasm");
		bcl_tools_prefix = Path.Combine (bcl_root, "wasm_tools");
		bcl_facades_prefix = Path.Combine (bcl_prefix, "Facades");
		bcl_prefixes = new List<string> ();
		if (is_netcore) {
			/* corelib */
			bcl_prefixes.Add (Path.Combine (bcl_root, "netcore"));
			/* .net runtime */
			bcl_prefixes.Add (netcore_sdkdir);
		} else {
			bcl_prefixes.Add (bcl_prefix);
		}

		foreach (var ra in root_assemblies) {
			AssemblyKind kind;
			var resolved = Resolve (ra, out kind);
			Import (resolved, kind);
		}
		if (add_binding) {
			var bindings = ResolveFramework (BINDINGS_ASM_NAME + ".dll");
			Import (bindings, AssemblyKind.Framework);
			var http = ResolveFramework (HTTP_ASM_NAME + ".dll");
			Import (http, AssemblyKind.Framework);
			var websockets = ResolveFramework (WEBSOCKETS_ASM_NAME + ".dll");
			Import (websockets, AssemblyKind.Framework);
		}

		if (enable_aot) {
			var to_aot = new Dictionary<string, bool> ();
			to_aot ["mscorlib"] = true;
			if (aot_assemblies != "") {
				foreach (var s in aot_assemblies.Split (','))
					to_aot [s] = true;
			}
			foreach (var ass in assemblies) {
				if (aot_assemblies == "" || to_aot.ContainsKey (ass.name)) {
					ass.aot = true;
					to_aot.Remove (ass.name);
				}
			}
			if (to_aot.Count > 0) {
				Console.Error.WriteLine ("Unknown assembly name '" + to_aot.Keys.ToArray ()[0] + "' in --aot-assemblies option.");
				return 1;
			}
		}

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

		// the linker does not consider these core by default
		var wasm_core_assemblies = new Dictionary<string, bool> ();
		if (add_binding) {		
			wasm_core_assemblies [BINDINGS_ASM_NAME] = true;
			wasm_core_assemblies [HTTP_ASM_NAME] = true;
			wasm_core_assemblies [WEBSOCKETS_ASM_NAME] = true;
		}
		// wasm core bindings module
		var wasm_core_bindings = string.Empty;
		if (add_binding) {
			wasm_core_bindings = BINDINGS_MODULE;
		}
		// wasm core bindings support file
		var wasm_core_support = string.Empty;
		var wasm_core_support_library = string.Empty;
		if (add_binding) {
			wasm_core_support = BINDINGS_MODULE_SUPPORT;
			wasm_core_support_library = $"--js-library {BINDINGS_MODULE_SUPPORT}";
		}
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
				else {
					var runtime_gen = "\nvar Module = {\n\tonRuntimeInitialized: function () {\n\t\tMONO.mono_load_runtime_and_bcl (\n\t\tconfig.vfs_prefix,\n\t\tconfig.deploy_prefix,\n\t\tconfig.enable_debugging,\n\t\tconfig.file_list,\n\t\tfunction () {\n\t\t\tApp.init ();\n\t\t}\n\t)\n\t},\n};";
					File.Delete (runtime_js);
					File.WriteAllText (runtime_js, runtime_gen);
				}
			}
		}

		AssemblyData dedup_asm = null;

		if (enable_dedup) {
			dedup_asm = new AssemblyData () { name = "aot-dummy",
					filename = "aot-dummy.dll",
					bc_path = "$builddir/aot-dummy.dll.bc",
					o_path = "$builddir/aot-dummy.dll.o",
					app_path = "$appdir/$deploy_prefix/aot-dummy.dll",
					linkout_path = "$builddir/linker-out/aot-dummy.dll",
					aot = true
					};
			assemblies.Add (dedup_asm);
			file_list.Add ("aot-dummy.dll");
		}

		var file_list_str = string.Join (",", file_list.Select (f => $"\"{Path.GetFileName (f)}\"").Distinct());
		var config = String.Format ("config = {{\n \tvfs_prefix: \"{0}\",\n \tdeploy_prefix: \"{1}\",\n \tenable_debugging: {2},\n \tfile_list: [ {3} ],\n", vfs_prefix, deploy_prefix, enable_debug ? "1" : "0", file_list_str);
		config += "}\n";
		var config_js = Path.Combine (emit_ninja ? builddir : out_prefix, "mono-config.js");
		File.Delete (config_js);
		File.WriteAllText (config_js, config);

		string runtime_dir;
		if (is_netcore)
			runtime_dir = Path.Combine (tool_prefix, use_release_runtime ? "builds/netcore-release" : "builds/netcore-debug");
		else if (enable_threads)
			runtime_dir = Path.Combine (tool_prefix, use_release_runtime ? "builds/threads-release" : "builds/threads-debug");
		else
			runtime_dir = Path.Combine (tool_prefix, use_release_runtime ? "builds/release" : "builds/debug");
		if (!emit_ninja) {
			var interp_files = new List<string> { "mono.js", "mono.wasm" };
			if (enable_threads) {
				interp_files.Add ("mono.worker.js");
				interp_files.Add ("mono.js.mem");
			}
			foreach (var fname in interp_files) {
				File.Delete (Path.Combine (out_prefix, fname));
				File.Copy (
						   Path.Combine (runtime_dir, fname),
						   Path.Combine (out_prefix, fname));
			}

			foreach(var asset in assets) {
				CopyFile (asset, 
						Path.Combine (out_prefix, Path.GetFileName (asset)), copyType, "Asset: ");
			}
		}

		if (!emit_ninja)
			return 0;

		if (build_wasm) {
			if (sdkdir == null) {
				Console.WriteLine ("The --mono-sdkdir argument is required.");
				return 1;
			}
			if (emscripten_sdkdir == null) {
				Console.WriteLine ("The --emscripten-sdkdir argument is required.");
				return 1;
			}
			GenDriver (builddir, profilers, ee_mode, link_icalls);
		}

		string runtime_libs = "";
		if (ee_mode == ExecMode.Interp || ee_mode == ExecMode.AotInterp || link_icalls) {
			runtime_libs += "$mono_sdkdir/wasm-runtime-release/lib/libmono-ee-interp.a $mono_sdkdir/wasm-runtime-release/lib/libmono-ilgen.a ";
			// We need to link the icall table because the interpreter uses it to lookup icalls even if the aot-ed icall wrappers are available
			if (!link_icalls)
				runtime_libs += "$mono_sdkdir/wasm-runtime-release/lib/libmono-icall-table.a ";
		}
		runtime_libs += "$mono_sdkdir/wasm-runtime-release/lib/libmonosgen-2.0.a";

		string aot_args = "llvm-path=$emscripten_sdkdir/upstream/bin,";
		string profiler_libs = "";
		string profiler_aot_args = "";
		foreach (var profiler in profilers) {
			profiler_libs += $"$mono_sdkdir/wasm-runtime-release/lib/libmono-profiler-{profiler}-static.a ";
			if (profiler_aot_args != "")
				profiler_aot_args += " ";
			profiler_aot_args += $"--profile={profiler}";
		}
		if (aot_profile != null) {
			CopyFile (aot_profile, Path.Combine (builddir, Path.GetFileName (aot_profile)), CopyType.IfNewer, "");
			aot_args += $"profile={aot_profile},profile-only,";
		}
		if (ee_mode == ExecMode.AotInterp)
			aot_args += "interp,";
		if (build_wasm)
			enable_zlib = true;

		runtime_dir = Path.GetFullPath (runtime_dir);
		sdkdir = Path.GetFullPath (sdkdir);
		out_prefix = Path.GetFullPath (out_prefix);

		string driver_deps = "";
		if (link_icalls)
			driver_deps += "$builddir/icall-table.h";
		if (gen_pinvoke)
			driver_deps += "$builddir/pinvoke-table.h";
		string emcc_flags = "";
		if (enable_lto)
			emcc_flags += "--llvm-lto 1 ";
		if (enable_zlib)
			emcc_flags += "-s USE_ZLIB=1 ";
		string emcc_link_flags = "";
		if (enable_debug)
			emcc_link_flags += "-O0 ";

		string strip_cmd = "";
		if (opts.NativeStrip)
			strip_cmd = " && $wasm_strip $out_wasm";

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
		ninja.WriteLine ($"bcl_facades_dir = {bcl_facades_prefix}");
		ninja.WriteLine ($"tools_dir = {bcl_tools_prefix}");
		ninja.WriteLine ($"emsdk_env = $builddir/emsdk_env.sh");
		if (add_binding) {
			ninja.WriteLine ($"wasm_core_bindings = $builddir/{BINDINGS_MODULE}");
			ninja.WriteLine ($"wasm_core_support = {wasm_core_support}");
			ninja.WriteLine ($"wasm_core_support_library = {wasm_core_support_library}");
		} else {
			ninja.WriteLine ("wasm_core_bindings =");
			ninja.WriteLine ("wasm_core_support =");
			ninja.WriteLine ("wasm_core_support_library =");
		}
		ninja.WriteLine ("cross = $mono_sdkdir/wasm-cross-release/bin/wasm32-unknown-none-mono-sgen");
		ninja.WriteLine ("emcc = source $emsdk_env && emcc");
		ninja.WriteLine ("wasm_strip = $emscripten_sdkdir/upstream/bin/wasm-strip");
		// -s ASSERTIONS=2 is very slow
		ninja.WriteLine ($"emcc_flags = -Oz -g {emcc_flags}-s DISABLE_EXCEPTION_CATCHING=0 -s ASSERTIONS=1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s NO_EXIT_RUNTIME=1 -s ERROR_ON_UNDEFINED_SYMBOLS=1 -s \"EXTRA_EXPORTED_RUNTIME_METHODS=[\'ccall\', \'cwrap\', \'setValue\', \'getValue\', \'UTF8ToString\']\" -s \"EXPORTED_FUNCTIONS=[\'___cxa_is_pointer_type\', \'___cxa_can_catch\']\" -s \"DEFAULT_LIBRARY_FUNCS_TO_INCLUDE=[\'setThrew\', \'memset\']\"");
		ninja.WriteLine ($"aot_base_args = llvmonly,asmonly,no-opt,static,direct-icalls,deterministic,{aot_args}");

		// Rules
		ninja.WriteLine ("rule aot");
		ninja.WriteLine ($"  command = MONO_PATH=$mono_path $cross --debug {profiler_aot_args} --aot=$aot_args,$aot_base_args,depfile=$depfile,llvm-outfile=$outfile $src_file");
		ninja.WriteLine ("  description = [AOT] $src_file -> $outfile");
		ninja.WriteLine ("rule aot-instances");
		ninja.WriteLine ($"  command = MONO_PATH=$mono_path $cross --debug {profiler_aot_args} --aot=$aot_base_args,llvm-outfile=$outfile,dedup-include=$dedup_image $src_files");
		ninja.WriteLine ("  description = [AOT-INSTANCES] $outfile");
		ninja.WriteLine ("rule mkdir");
		ninja.WriteLine ("  command = mkdir -p $out");
		ninja.WriteLine ("rule cp");
		ninja.WriteLine ("  command = cp $in $out");
		// Copy $in to $out only if it changed
		ninja.WriteLine ("rule cpifdiff");
		ninja.WriteLine ("  command = if cmp -s $in $out ; then : ; else cp $in $out ; fi");
		ninja.WriteLine ("  restat = true");
		ninja.WriteLine ("  description = [CPIFDIFF] $in -> $out");
		ninja.WriteLine ("rule create-emsdk-env");
		ninja.WriteLine ("  command = $emscripten_sdkdir/emsdk construct_env $out");
		ninja.WriteLine ("rule emcc");
		ninja.WriteLine ("  command = bash -c '$emcc $emcc_flags $flags -c -o $out $in'");
		ninja.WriteLine ("  description = [EMCC] $in -> $out");
		ninja.WriteLine ("rule emcc-link");
		ninja.WriteLine ($"  command = bash -c '$emcc $emcc_flags {emcc_link_flags} -o $out_js --js-library $tool_prefix/src/library_mono.js --js-library $tool_prefix/src/dotnet_support.js {wasm_core_support_library} $in' {strip_cmd}");
		ninja.WriteLine ("  description = [EMCC-LINK] $in -> $out_js");
		ninja.WriteLine ("rule linker");
		ninja.WriteLine ("  command = mono $tools_dir/monolinker.exe -out $builddir/linker-out -l none --deterministic --explicit-reflection --disable-opt unreachablebodies --exclude-feature com --exclude-feature remoting --exclude-feature etw $linker_args || exit 1; for f in $out; do if test ! -f $$f; then echo > empty.cs; csc /deterministic /nologo /out:$$f /target:library empty.cs; else touch $$f; fi; done");
		ninja.WriteLine ("  description = [IL-LINK]");
		ninja.WriteLine ("rule aot-dummy");
		ninja.WriteLine ("  command = echo > aot-dummy.cs; csc /deterministic /out:$out /target:library aot-dummy.cs");
		ninja.WriteLine ("rule gen-runtime-icall-table");
		ninja.WriteLine ("  command = $cross --print-icall-table > $out");
		ninja.WriteLine ("rule gen-icall-table");
		ninja.WriteLine ("  command = mono $tools_dir/wasm-tuner.exe --gen-icall-table $runtime_table $in > $out");
		ninja.WriteLine ("rule gen-pinvoke-table");
		ninja.WriteLine ("  command = mono $tools_dir/wasm-tuner.exe --gen-pinvoke-table $pinvoke_libs $in > $out");
		ninja.WriteLine ("rule ilstrip");
		ninja.WriteLine ("  command = cp $in $out; mono $tools_dir/mono-cil-strip.exe $out");
		ninja.WriteLine ("  description = [IL-STRIP]");

		// Targets
		ninja.WriteLine ("build $appdir: mkdir");
		ninja.WriteLine ("build $appdir/$deploy_prefix: mkdir");
		ninja.WriteLine ("build $appdir/runtime.js: cpifdiff $builddir/runtime.js");
		ninja.WriteLine ("build $appdir/mono-config.js: cpifdiff $builddir/mono-config.js");
		if (build_wasm) {
			var source_file = Path.GetFullPath (Path.Combine (tool_prefix, "src", "driver.c"));
			ninja.WriteLine ($"build $builddir/driver.c: cpifdiff {source_file}");
			ninja.WriteLine ($"build $builddir/driver-gen.c: cpifdiff $builddir/driver-gen.c.in");

			var pinvoke_file = Path.GetFullPath (Path.Combine (tool_prefix, "src", "pinvoke-tables-default.h"));
			ninja.WriteLine ($"build $builddir/pinvoke-tables-default.h: cpifdiff {pinvoke_file}");
			driver_deps += $" $builddir/pinvoke-tables-default.h";

			var driver_cflags = enable_aot ? "-DENABLE_AOT=1" : "";

			if (add_binding) {
				var bindings_source_file = Path.GetFullPath (Path.Combine (tool_prefix, "src", "corebindings.c"));
				ninja.WriteLine ($"build $builddir/corebindings.c: cpifdiff {bindings_source_file}");

				ninja.WriteLine ($"build $builddir/corebindings.o: emcc $builddir/corebindings.c | $emsdk_env");
				ninja.WriteLine ($"  flags = -I$mono_sdkdir/wasm-runtime-release/include/mono-2.0");
				driver_cflags += " -DCORE_BINDINGS ";
			}
			if (gen_pinvoke)
				driver_cflags += " -DGEN_PINVOKE ";

			ninja.WriteLine ("build $emsdk_env: create-emsdk-env");
			ninja.WriteLine ($"build $builddir/driver.o: emcc $builddir/driver.c | $emsdk_env $builddir/driver-gen.c {driver_deps}");
			ninja.WriteLine ($"  flags = {driver_cflags} -DDRIVER_GEN=1 -I$mono_sdkdir/wasm-runtime-release/include/mono-2.0");

			if (enable_zlib) {
				var zlib_source_file = Path.GetFullPath (Path.Combine (tool_prefix, "src", "zlib-helper.c"));
				ninja.WriteLine ($"build $builddir/zlib-helper.c: cpifdiff {zlib_source_file}");

				ninja.WriteLine ($"build $builddir/zlib-helper.o: emcc $builddir/zlib-helper.c | $emsdk_env");
				ninja.WriteLine ($"  flags = -I$mono_sdkdir/wasm-runtime-release/include/mono-2.0 -I$mono_sdkdir/wasm-runtime-release/include/support");
			}
		} else {
			ninja.WriteLine ("build $appdir/mono.js: cpifdiff $wasm_runtime_dir/mono.js");
			ninja.WriteLine ("build $appdir/mono.wasm: cpifdiff $wasm_runtime_dir/mono.wasm");
			if (enable_threads) {
				ninja.WriteLine ("build $appdir/mono.worker.js: cpifdiff $wasm_runtime_dir/mono.worker.js");
				ninja.WriteLine ("build $appdir/mono.js.mem: cpifdiff $wasm_runtime_dir/mono.js.mem");
			}
		}
		if (enable_aot)
			ninja.WriteLine ("build $builddir/aot-in: mkdir");

		var ofiles = "";
		var bc_files = "";
		string linker_infiles = "";
		string linker_ofiles = "";
		string dedup_infiles = "";
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
			string filename_pdb = Path.ChangeExtension (filename, "pdb");
			var source_file_path = Path.GetFullPath (assembly);
			var source_file_path_pdb = Path.ChangeExtension (source_file_path, "pdb");
			string infile = "";
			string infile_pdb = "";
			bool emit_pdb = assemblies_with_dbg_info.Contains (source_file_path_pdb);
			if (enable_linker) {
				a.linkin_path = $"$builddir/linker-in/{filename}";
				a.linkout_path = $"$builddir/linker-out/{filename}";
				linker_infiles += $"{a.linkin_path} ";
				linker_ofiles += $" {a.linkout_path}";
				ninja.WriteLine ($"build {a.linkin_path}: cp {source_file_path}");
				a.aotin_path = a.linkout_path;
				infile = $"{a.aotin_path}";
			} else {
				infile = $"$builddir/{filename}";
				ninja.WriteLine ($"build $builddir/{filename}: cpifdiff {source_file_path}");
				if (emit_pdb) {
					ninja.WriteLine ($"build $builddir/{filename_pdb}: cpifdiff {source_file_path_pdb}");
					infile_pdb = $"$builddir/{filename_pdb}";
				}
			}

			a.final_path = infile;
			if (il_strip) {
				ninja.WriteLine ($"build $builddir/ilstrip-out/{filename} : ilstrip {infile}");
				a.final_path = $"$builddir/ilstrip-out/{filename}";
			}

			ninja.WriteLine ($"build $appdir/$deploy_prefix/{filename}: cpifdiff {a.final_path}");
			if (emit_pdb && infile_pdb != "")
				ninja.WriteLine ($"build $appdir/$deploy_prefix/{filename_pdb}: cpifdiff {infile_pdb}");
			if (a.aot) {
				a.bc_path = $"$builddir/{filename}.bc";
				a.o_path = $"$builddir/{filename}.o";
				a.aot_depfile_path = $"$builddir/linker-out/{filename}.depfile";

				if (filename == "mscorlib.dll") {
					// mscorlib has no dependencies so we can skip the aot step if the input didn't change
					// The other assemblies depend on their references
					infile = "$builddir/aot-in/mscorlib.dll";
					a.aotin_path = infile;
					ninja.WriteLine ($"build {a.aotin_path}: cpifdiff {a.linkout_path}");
				}
				ninja.WriteLine ($"build {a.bc_path}.tmp: aot {infile}");
				ninja.WriteLine ($"  src_file={infile}");
				ninja.WriteLine ($"  outfile={a.bc_path}.tmp");
				ninja.WriteLine ($"  mono_path=$builddir/aot-in:{aot_in_path}");
				ninja.WriteLine ($"  depfile={a.aot_depfile_path}");
				if (enable_dedup)
					ninja.WriteLine ($"  aot_args=dedup-skip");

				ninja.WriteLine ($"build {a.bc_path}: cpifdiff {a.bc_path}.tmp");
				ninja.WriteLine ($"build {a.o_path}: emcc {a.bc_path} | $emsdk_env");

				ofiles += " " + $"{a.o_path}";
				bc_files += " " + $"{a.bc_path}";
				dedup_infiles += $" {a.aotin_path}";
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
			ninja.WriteLine ($"build {a.bc_path}.tmp: aot-instances | {bc_files} {a.linkout_path}");
			ninja.WriteLine ($"  dedup_image={a.filename}");
			ninja.WriteLine ($"  src_files={dedup_infiles} {a.linkout_path}");
			ninja.WriteLine ($"  outfile={a.bc_path}.tmp");
			ninja.WriteLine ($"  mono_path=$builddir/aot-in:{aot_in_path}");
			ninja.WriteLine ($"build {a.app_path}: cpifdiff {a.linkout_path}");
			ninja.WriteLine ($"build {a.linkout_path}: aot-dummy");
			// The dedup image might not have changed
			ninja.WriteLine ($"build {a.bc_path}: cpifdiff {a.bc_path}.tmp");
			ninja.WriteLine ($"build {a.o_path}: emcc {a.bc_path} | $emsdk_env");
			ofiles += $" {a.o_path}";
		}
		if (link_icalls) {
			string icall_assemblies = "";
			foreach (var a in assemblies) {
				if (a.name == "mscorlib" || a.name == "System")
					icall_assemblies += $"{a.linkout_path} ";
			}
			ninja.WriteLine ("build $builddir/icall-table.json: gen-runtime-icall-table");
			ninja.WriteLine ($"build $builddir/icall-table.h: gen-icall-table {icall_assemblies}");
			ninja.WriteLine ($"  runtime_table=$builddir/icall-table.json");
		}
		if (gen_pinvoke) {
			string pinvoke_assemblies = "";
			foreach (var a in assemblies)
				pinvoke_assemblies += $"{a.linkout_path} ";
			ninja.WriteLine ($"build $builddir/pinvoke-table.h: gen-pinvoke-table {pinvoke_assemblies}");
			ninja.WriteLine ($"  pinvoke_libs=System.Native,{pinvoke_libs}");
		}
		if (build_wasm) {
			string zlibhelper = enable_zlib ? "$builddir/zlib-helper.o" : "";
			ninja.WriteLine ($"build $appdir/mono.js $appdir/mono.wasm: emcc-link $builddir/driver.o {zlibhelper} {wasm_core_bindings} {ofiles} {profiler_libs} {runtime_libs} $mono_sdkdir/wasm-runtime-release/lib/libmono-native.a | $tool_prefix/src/library_mono.js $tool_prefix/src/dotnet_support.js {wasm_core_support} $emsdk_env");
			ninja.WriteLine ("  out_js=$appdir/mono.js");
			ninja.WriteLine ("  out_wasm=$appdir/mono.wasm");
		}
		if (enable_linker) {
			switch (linkMode) {
			case LinkMode.SdkOnly:
				coremode = "link";
				usermode = "copy";
				break;
			case LinkMode.All:
				coremode = "link";
				usermode = "link";
				break;
			default:
				coremode = "link";
				usermode = "link";
				break;
			}

			string linker_args = "";
			if (!string.IsNullOrEmpty (linkDescriptor)) {
				linker_args += $"-x {linkDescriptor} ";
				foreach (var assembly in root_assemblies) {
					string filename = Path.GetFileName (assembly);
					linker_args += $"-p {usermode} {filename} -r linker-in/{filename} ";
				}
			} else {
				foreach (var assembly in root_assemblies) {
					string filename = Path.GetFileName (assembly);
					linker_args += $"-a linker-in/{filename} ";
				}
			}

			// the linker does not consider these core by default
			foreach (var assembly in wasm_core_assemblies.Keys) {
				linker_args += $"-p {coremode} {assembly} ";
			}
			if (linker_verbose) {
				linker_args += "--verbose ";
			}
			linker_args += $"-d linker-in -d $bcl_dir -d $bcl_facades_dir -c {coremode} -u {usermode} ";
			foreach (var assembly in wasm_core_assemblies.Keys) {
				linker_args += $"-r {assembly} ";
			}

			ninja.WriteLine ("build $builddir/linker-out: mkdir");
			ninja.WriteLine ($"build {linker_ofiles}: linker {linker_infiles}");
			ninja.WriteLine ($"  linker_args={linker_args}");
		}
		if (il_strip)
			ninja.WriteLine ("build $builddir/ilstrip-out: mkdir");

		foreach(var asset in assets) {
			var filename = Path.GetFileName (asset);
			var abs_path = Path.GetFullPath (asset);
			ninja.WriteLine ($"build $appdir/{filename}: cpifdiff {abs_path}");
		}

		ninja.Close ();

		return 0;
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
