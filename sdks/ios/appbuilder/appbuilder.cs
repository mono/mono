using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mono.Options;

public class AppBuilder
{
	public static void Main (String[] args) {
		new AppBuilder ().Run (args);
	}

	void GenMain (string builddir, List<string> assembly_names, bool isinterp) {
		var symbols = new List<string> ();
		foreach (var img in assembly_names) {
			symbols.Add (String.Format ("mono_aot_module_{0}_info", img.Replace ('.', '_').Replace ('-', '_')));
		}

		var w = File.CreateText (Path.Combine (builddir, "modules.m"));

		/* copy from <mono/mini/jit.h> */
		w.WriteLine ("typedef enum {");
		w.WriteLine ("	MONO_AOT_MODE_NONE,");
		w.WriteLine ("	MONO_AOT_MODE_NORMAL,");
		w.WriteLine ("	MONO_AOT_MODE_HYBRID,");
		w.WriteLine ("	MONO_AOT_MODE_FULL,");
		w.WriteLine ("	MONO_AOT_MODE_LLVMONLY,");
		w.WriteLine ("	MONO_AOT_MODE_INTERP,");
		w.WriteLine ("	MONO_AOT_MODE_INTERP_LLVMONLY");
		w.WriteLine ("} MonoAotMode;");
		w.WriteLine ();
		w.WriteLine ("void mono_jit_set_aot_mode (MonoAotMode mode);");
		w.WriteLine ();

		w.WriteLine ("extern void mono_aot_register_module (char *name);");
		w.WriteLine ();

		if (isinterp) {
			w.WriteLine ("extern void mono_ee_interp_init (const char *);");
			w.WriteLine ("extern void mono_icall_table_init (void);");
			w.WriteLine ("extern void mono_marshal_ilgen_init (void);");
			w.WriteLine ("extern void mono_method_builder_ilgen_init (void);");
			w.WriteLine ("extern void mono_sgen_mono_ilgen_init (void);");
			w.WriteLine ();
		}

		foreach (var symbol in symbols) {
			w.WriteLine ($"extern void *{symbol};");
		}

		w.WriteLine ();
		w.WriteLine ("void mono_ios_register_modules (void)");
		w.WriteLine ("{");
		foreach (var symbol in symbols) {
			w.WriteLine ($"\tmono_aot_register_module ({symbol});");
		}
		w.WriteLine ("}");

		w.WriteLine ();
		w.WriteLine ("void mono_ios_setup_execution_mode (void)");
		w.WriteLine ("{");
		if (isinterp) {
			w.WriteLine ("\tmono_icall_table_init ();");
			w.WriteLine ("\tmono_marshal_ilgen_init ();");
			w.WriteLine ("\tmono_method_builder_ilgen_init ();");
			w.WriteLine ("\tmono_sgen_mono_ilgen_init ();");
			w.WriteLine ("\tmono_ee_interp_init (0);");
		}
		w.WriteLine ("\tmono_jit_set_aot_mode ({0});", isinterp ? "MONO_AOT_MODE_INTERP" : "MONO_AOT_MODE_FULL");
		w.WriteLine ("}");

		w.WriteLine ();
		w.Close ();
	}

	void check_mandatory (string val, string name) {
		if (val == null) {
			Console.Error.WriteLine ($"The {name} argument is mandatory.");
			Environment.Exit (1);
		}
	}

	void Run (String[] args) {
		string target = null;
		string appdir = null;
		string builddir = null;
		string apptemplatedir = null;
		string mono_sdkdir = null;
		string bundle_identifier = null;
		string bundle_name = null;
		string bundle_executable = null;
		string sysroot = null;
		string aotdir = null;
		string exe = null;
		string signing_identity = null;
		string team_identifier = null;
		bool isnetcore = false;
		bool isdev = false;
		bool isdebug = false;
		bool isllvm = false;
		bool isinterponly = false;
		bool isinterpmixed = false;
		var assemblies = new List<string> ();
		var content_files = new List<string> ();
		var p = new OptionSet () {
				{ "target=", s => target = s },
				{ "appdir=", s => appdir = s },
				{ "builddir=", s => builddir = s },
				{ "apptemplatedir=", s => apptemplatedir = s },
				{ "mono-sdkdir=", s => mono_sdkdir = s },
				{ "sysroot=", s => sysroot = s },
				{ "aot-cachedir=", s => aotdir = s },
				{ "bundle-identifier=", s => bundle_identifier = s },
				{ "bundle-name=", s => bundle_name = s },
				{ "bundle-executable=", s => bundle_executable = s },
				{ "signing-identity=", s => signing_identity = s },
				{ "team-identifier=", s => team_identifier = s },
				{ "llvm", s => isllvm = true },
				{ "netcore", s => isnetcore = true },
				{ "debug", s => isdebug = true },
				{ "interp-only", s => isinterponly = true },
				{ "interp-mixed", s => isinterpmixed = true },
				{ "exe=", s => exe = s },
				{ "r=", s => assemblies.Add (s) },
				{ "content=", s => content_files.Add (s) },
			};

		var new_args = p.Parse (args).ToArray ();

		check_mandatory (target, "--target");
		check_mandatory (apptemplatedir, "--apptemplatedir");
		check_mandatory (appdir, "--appdir");
		check_mandatory (mono_sdkdir, "--mono-sdkdir");
		check_mandatory (sysroot, "--sysroot");
		check_mandatory (signing_identity, "--signing-identity");

		switch (target) {
		case "ios-dev64":
			isdev = true;
			check_mandatory (team_identifier, "--team-identifier");
			break;
		case "ios-sim64":
			break;
		default:
			Console.WriteLine ($"Possible values for the '--target=' argument are 'ios-dev64', 'ios-sim64', got {target}.");
			Environment.Exit (1);
			break;
		}

		string runtime = isnetcore ? "ios-netcore_target64-release" : "ios-target64-release";
		string cross_runtime = isnetcore ? "ios-netcore_cross64-release" : "ios-cross64-release";

		bool isinterpany = isinterponly || isinterpmixed;

		string aot_args = "";
		string cross_runtime_args = "";

		if (isinterponly) {
			aot_args = "interp";
		} else if (isinterpmixed) {
			aot_args = "interp,full";
		} else {
			aot_args = "full";
		}

		if (isdebug)
			aot_args += ",soft-debug";
		if (isllvm) {
			cross_runtime_args = "--llvm";
			aot_args += ",llvm-path=$mono_sdkdir/llvm-llvm64/bin,llvm-outfile=$llvm_outfile";

			if (isdebug) {
				Console.WriteLine ("--debug can't be used together with --llvm");
				Environment.Exit (1);
			}
		}

		Directory.CreateDirectory (builddir);

		// Create Info.plist file
		var lines = File.ReadAllLines (Path.Combine (apptemplatedir, "Info.plist.in"));
		for (int i = 0; i < lines.Length; ++i) {
			string line = lines [i];
			line = line.Replace ("BUNDLE_IDENTIFIER", bundle_identifier);
			line = line.Replace ("BUNDLE_EXECUTABLE", bundle_executable);
			line = line.Replace ("BUNDLE_NAME", bundle_name);
			line = line.Replace ("BUNDLE_PLATFORM", isdev ? "iPhoneOS" : "iPhoneSimulator");
			line = line.Replace ("BUNDLE_DTPLATFORMNAME", isdev ? "iphoneos" : "iphonesimulator");
			lines [i] = line;
		}
		File.WriteAllLines (Path.Combine (builddir, "Info.plist"), lines);

		// Create Entitlements.plist file
		lines = File.ReadAllLines (Path.Combine (apptemplatedir, "Entitlements.plist.in"));
		for (int i = 0; i < lines.Length; ++i) {
			string line = lines [i];
			line = line.Replace ("TEAM_IDENTIFIER", team_identifier);
			lines [i] = line;
		}
		File.WriteAllLines (Path.Combine (builddir, "Entitlements.plist"), lines);

		// Create config.json file
		string config = "{ \"exe\" : \"" + exe + "\" }";
		File.WriteAllLines (Path.Combine (builddir, "config.json"), new string [] { config });

		var ninja = File.CreateText (Path.Combine (builddir, "build.ninja"));

		// Defines
		ninja.WriteLine ($"mono_sdkdir = {mono_sdkdir}");
		ninja.WriteLine ($"apptemplate_dir = {apptemplatedir}");
		ninja.WriteLine ($"appdir = {appdir}");
		ninja.WriteLine ($"sysroot = {sysroot}");
		ninja.WriteLine ($"cross = $mono_sdkdir/{cross_runtime}/bin/aarch64-darwin-mono-sgen");
		ninja.WriteLine ($"builddir = .");
		if (aotdir != null)
			ninja.WriteLine ($"aotdir = {aotdir}");
		ninja.WriteLine ($"signing_identity = {signing_identity}");
		ninja.WriteLine ("entitlements = $builddir/Entitlements.plist");
		// Rules
		ninja.WriteLine ("rule aot");
		ninja.WriteLine ($"  command = MONO_PATH=$mono_path $cross -O=gsharedvt,float32 --debug {cross_runtime_args} --aot=mtriple=arm64-ios,static,asmonly,direct-icalls,no-direct-calls,dwarfdebug,{aot_args},outfile=$outfile,data-outfile=$data_outfile $src_file");
		ninja.WriteLine ("  description = [AOT] $src_file -> $outfile");
		// ninja remakes files it hadn't seen before even if the timestamp is newer, so have to add a test ourselves
		ninja.WriteLine ("rule aot-cached");
		ninja.WriteLine ($"  command = if ! test -f $outfile; then MONO_PATH=$mono_path $cross -O=gsharedvt,float32 --debug {cross_runtime_args} --aot=mtriple=arm64-ios,static,asmonly,direct-icalls,no-direct-calls,dwarfdebug,{aot_args},outfile=$outfile,data-outfile=$data_outfile $src_file; fi");
		ninja.WriteLine ("  description = [AOT] $src_file -> $outfile");
		ninja.WriteLine ("rule assemble");
		ninja.WriteLine ("  command = clang -isysroot $sysroot -miphoneos-version-min=10.1 -arch arm64 -c -o $out $in");
		ninja.WriteLine ("  description = [ASM] $in -> $out");
		ninja.WriteLine ("rule assemble-cached");
		ninja.WriteLine ("  command = if ! test -f $out; then clang -isysroot $sysroot -miphoneos-version-min=10.1 -arch arm64 -c -o $out $in; fi");
		ninja.WriteLine ("  description = [ASM] $in -> $out");
		ninja.WriteLine ("rule cp");
		ninja.WriteLine ("  command = cp $in $out");
		ninja.WriteLine ("  description = [CP] $in -> $out");
		ninja.WriteLine ("rule cp-recursive");
		ninja.WriteLine ("  command = cp -r $in $out");
		ninja.WriteLine ("rule cpifdiff");
		ninja.WriteLine ("  command = if cmp -s $in $out ; then : ; else cp $in $out ; fi");
		ninja.WriteLine ("  restat = true");
		ninja.WriteLine ("rule plutil");
		ninja.WriteLine ("  command = cp $in $out; plutil -convert binary1 $out");
		ninja.WriteLine ("rule codesign");
		ninja.WriteLine ("  command = codesign -v --force --sign \"$signing_identity\" --entitlements $entitlements --timestamp=none $in");
		ninja.WriteLine ("rule codesign-sim");
		ninja.WriteLine ("  command = codesign --force --sign - --timestamp=none $in");
		ninja.WriteLine ("rule mkdir");
		ninja.WriteLine ("  command = mkdir -p $out");
		ninja.WriteLine ("rule compile-objc");
		ninja.WriteLine ("  command = clang -isysroot $sysroot -miphoneos-version-min=10.1 -arch arm64 -c -o $out $in");
		ninja.WriteLine ("rule gen-exe");
		ninja.WriteLine ("  command = mkdir $appdir");
		ninja.WriteLine ($"  command = clang -ObjC -isysroot $sysroot -miphoneos-version-min=10.1 -arch arm64 -framework Foundation -framework UIKit -framework GSS -o $appdir/{bundle_executable} $in -liconv -lz $forcelibs");
	
		var ofiles = "";
		var assembly_names = new List<string> ();
		var cultures = CultureInfo.GetCultures (CultureTypes.AllCultures).Where (x => !String.IsNullOrEmpty (x.IetfLanguageTag)).Select (x => x.IetfLanguageTag).Distinct ();
		foreach (var assembly in assemblies) {
			string filename = Path.GetFileName (assembly);
			var filename_noext = Path.GetFileNameWithoutExtension (filename);

			File.Copy (assembly, Path.Combine (builddir, filename), true);
			if (aotdir != null && !File.Exists (Path.Combine (aotdir, filename)))
				/* Don't overwrite to avoid changing the timestamp */
				File.Copy (assembly, Path.Combine (aotdir, filename), false);

			ninja.WriteLine ($"build $appdir/{filename}: cpifdiff $builddir/{filename}");

			string pdb = Path.ChangeExtension (assembly, ".pdb");
			if (isdebug && File.Exists (pdb)) {
				File.Copy (pdb, Path.Combine (builddir, filename_noext + ".pdb"), true);
				ninja.WriteLine ($"build $appdir/{filename_noext}.pdb: cpifdiff $builddir/{filename_noext}.pdb");
			}

			var assembly_dir = Path.GetDirectoryName (assembly);
			var resource_filename = filename.Replace (".dll", ".resources.dll");
			foreach (var culture in cultures) {
				var resource_assembly = Path.Combine (assembly_dir, culture, resource_filename);
				if (!File.Exists(resource_assembly)) continue;

				Directory.CreateDirectory (Path.Combine (builddir, culture));
				File.Copy (resource_assembly, Path.Combine (builddir, culture, resource_filename), true);
				ninja.WriteLine ($"build $appdir/{culture}/{resource_filename}: cpifdiff $builddir/{culture}/{resource_filename}");
			}

			if (isinterpany && filename_noext != "mscorlib")
				continue;

			if (isdev) {
				string destdir = null;
				string srcfile = null;
				string assemble_rule = null;
				if (aotdir != null) {
					destdir = "$aotdir";
					srcfile = Path.Combine (aotdir, filename);
					assemble_rule = "assemble-cached";
				} else {
					destdir = "$builddir";
					srcfile = $"{filename}";
					assemble_rule = "assemble";
				}
				string outputs = $"{destdir}/{filename}.s {destdir}/{filename_noext}.aotdata";
				if (isllvm)
					outputs += $" {destdir}/{filename}.llvm.s";
				if (aotdir != null)
					ninja.WriteLine ($"build {outputs}: aot-cached {srcfile}");
				else
					ninja.WriteLine ($"build {outputs}: aot {srcfile}");
				ninja.WriteLine ($"  src_file={srcfile}");
				ninja.WriteLine ($"  outfile={destdir}/{filename}.s");
				ninja.WriteLine ($"  data_outfile={destdir}/{filename_noext}.aotdata");
				ninja.WriteLine ($"  mono_path={destdir}");
				if (isllvm)
					ninja.WriteLine ($"  llvm_outfile={destdir}/{filename}.llvm.s");
				ninja.WriteLine ($"build {destdir}/{filename}.o: {assemble_rule} {destdir}/{filename}.s");
				if (isllvm)
					ninja.WriteLine ($"build {destdir}/{filename}.llvm.o: {assemble_rule} {destdir}/{filename}.llvm.s");
				ninja.WriteLine ($"build $appdir/{filename_noext}.aotdata: cp {destdir}/{filename_noext}.aotdata");

				ofiles += " " + ($"{destdir}/{filename}.o");
				if (isllvm)
					ofiles += " " + ($"{destdir}/{filename}.llvm.o");
			}
			var aname = AssemblyName.GetAssemblyName (assembly);
			assembly_names.Add (aname.Name);
		}

		foreach (var file in content_files) {
			string filename = Path.GetFileName (file);
			ninja.WriteLine ($"build $appdir/{filename}: cpifdiff {file}");
		}

		ninja.WriteLine ("build $appdir: mkdir");

		if (isdev) {
			string libs = $"$mono_sdkdir/{runtime}/lib/libmonosgen-2.0.a";
			if (isinterpany) {
				libs += $" $mono_sdkdir/{runtime}/lib/libmono-ee-interp.a";
				libs += $" $mono_sdkdir/{runtime}/lib/libmono-icall-table.a";
				libs += $" $mono_sdkdir/{runtime}/lib/libmono-ilgen.a";
			}
			ninja.WriteLine ($"build $appdir/{bundle_executable}: gen-exe {ofiles} $builddir/modules.o " + libs + (isnetcore ? " $apptemplate_dir/app-netcore-device.a" : " $apptemplate_dir/app-device.a"));
			if (isnetcore)
				ninja.WriteLine ($"    forcelibs = -force_load $mono_sdkdir/{runtime}/lib/System.Native.a -force_load $mono_sdkdir/{runtime}/lib/System.IO.Compression.Native.a -force_load $mono_sdkdir/{runtime}/lib/System.Security.Cryptography.Native.Apple.a");
			else
				ninja.WriteLine ($"    forcelibs = -force_load $mono_sdkdir/{runtime}/lib/libmono-native-unified.a");
			ninja.WriteLine ("build $builddir/modules.o: compile-objc $builddir/modules.m");
		} else {
			ninja.WriteLine ($"build $appdir/{bundle_executable}: cp " + (isnetcore ? "$apptemplate_dir/app-netcore-simulator" : "$apptemplate_dir/app-simulator"));
		}
		ninja.WriteLine ("build $builddir/Info.plist.binary: plutil $builddir/Info.plist");
		ninja.WriteLine ("build $appdir/Info.plist: cpifdiff $builddir/Info.plist.binary");
		ninja.WriteLine ("build $appdir/config.json: cpifdiff $builddir/config.json");
		if (isdev)
			ninja.WriteLine ($"build $appdir/_CodeSignature: codesign $appdir/{bundle_executable}");
		else
			ninja.WriteLine ($"build $appdir/_CodeSignature: codesign-sim $appdir/{bundle_executable}");

		ninja.Close ();

		GenMain (builddir, assembly_names, isinterpany);
	}
}
