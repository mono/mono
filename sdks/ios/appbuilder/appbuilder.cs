using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Mono.Options;

public class AppBuilder
{
	public static void Main (String[] args) {
		new AppBuilder ().Run (args);
	}

	void GenMain (string builddir, List<string> assembly_names) {
		var symbols = new List<string> ();
		foreach (var img in assembly_names) {
			symbols.Add (String.Format ("mono_aot_module_{0}_info", img.Replace ('.', '_').Replace ('-', '_')));
		}

		var w = File.CreateText (Path.Combine (builddir, "main.m"));

		w.WriteLine ($"extern void mono_aot_register_module (char *name);");

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
		string runtimedir = null;
		string mono_sdkdir = null;
		string bundle_identifier = null;
		string bundle_name = null;
		string bundle_executable = null;
		string exe = null;
		bool isdev = false;
		var assemblies = new List<string> ();
		var p = new OptionSet () {
				{ "target=", s => target = s },
				{ "appdir=", s => appdir = s },
				{ "builddir=", s => builddir = s },
				{ "runtimedir=", s => runtimedir = s },
				{ "mono-sdkdir=", s => mono_sdkdir = s },
				{ "bundle-identifier=", s => bundle_identifier = s },
				{ "bundle-name=", s => bundle_name = s },
				{ "bundle-executable=", s => bundle_executable = s },
				{ "exe=", s => exe = s },
				{ "r=", s => assemblies.Add (s) },
			};

		var new_args = p.Parse (args).ToArray ();

		check_mandatory (target, "--target");
		check_mandatory (runtimedir, "--runtimedir");
		check_mandatory (appdir, "--appdir");
		check_mandatory (mono_sdkdir, "--mono-sdkdir");

		switch (target) {
		case "ios-dev64":
			isdev = true;
			break;
		case "ios-sim64":
			break;
		default:
			Console.WriteLine ($"Possible values for the '--target=' argument are 'ios-dev64', 'ios-sim64', got {target}.");
			Environment.Exit (1);
			break;
		}

		Directory.CreateDirectory (builddir);

		// Create Info.plist file
		var lines = File.ReadAllLines (Path.Combine (runtimedir, "Info.plist.in"));
		for (int i = 0; i < lines.Length; ++i) {
			string line = lines [i];
			line = line.Replace ("BUNDLE_IDENTIFIER", bundle_identifier);
			line = line.Replace ("BUNDLE_EXECUTABLE", bundle_executable);
			line = line.Replace ("BUNDLE_NAME", bundle_name);
			line = line.Replace ("PLATFORM", isdev ? "iPhoneOS" : "iPhoneSimulator");
			lines [i] = line;
		}
		File.WriteAllLines (Path.Combine (builddir, "Info.plist"), lines);

		// Create config.json file
		string config = "{ \"exe\" : \"" + exe + "\" }";
		File.WriteAllLines (Path.Combine (builddir, "config.json"), new string [] { config });

		var ninja = File.CreateText (Path.Combine (builddir, "build.ninja"));

		// Defines
		ninja.WriteLine ($"mono_sdkdir = {mono_sdkdir}");
		ninja.WriteLine ($"monoios_dir = {runtimedir}");
		ninja.WriteLine ($"appdir = {appdir}");
		ninja.WriteLine ("sysroot = /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk");
		ninja.WriteLine ("cross = $mono_sdkdir/ios-cross64/bin/aarch64-darwin-mono-sgen");
		ninja.WriteLine ($"builddir = .");
		// Rules
		ninja.WriteLine ("rule aot");
		ninja.WriteLine ("  command = MONO_PATH=$builddir $cross -O=gsharedvt,float32 --debug --aot=full,static,asmonly,direct-icalls,no-direct-calls,soft-debug,dwarfdebug,outfile=$outfile,data-outfile=$data_outfile $src_file");
		ninja.WriteLine ("  description = [AOT] $src_file -> $outfile");
		ninja.WriteLine ("rule assemble");
		ninja.WriteLine ("  command = clang -isysroot $sysroot -miphoneos-version-min=10.1 -arch arm64 -c -o $out $in");
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
		ninja.WriteLine ("  command = codesign -v --force --sign EFB309D105E514963C149962F8F4F07479BB4100 --entitlements $entitlements --timestamp=none $in");
		ninja.WriteLine ("rule codesign-sim");
		ninja.WriteLine ("  command = codesign --force --sign - --timestamp=none $in");
		ninja.WriteLine ("rule mkdir");
		ninja.WriteLine ("  command = mkdir -p $out");
		ninja.WriteLine ("rule compile-objc");
		ninja.WriteLine ("  command = clang -isysroot $sysroot -miphoneos-version-min=10.1 -arch arm64 -c -o $out $in");
		ninja.WriteLine ("rule gen-exe");
		ninja.WriteLine ("  command = mkdir $appdir");
		ninja.WriteLine ($"  command = clang -ObjC -isysroot $sysroot -miphoneos-version-min=10.1 -arch arm64 -framework Foundation -framework UIKit -o $appdir/{bundle_executable} $in -liconv");
	
		var ofiles = "";
		var assembly_names = new List<string> ();
		foreach (var assembly in assemblies) {
			string filename = Path.GetFileName (assembly);
			var filename_noext = Path.GetFileNameWithoutExtension (filename);

			File.Copy (assembly, Path.Combine (builddir, filename), true);

			ninja.WriteLine ($"build $appdir/{filename}: cpifdiff $builddir/{filename}");
			if (isdev) {
				ninja.WriteLine ($"build $builddir/{filename}.s $builddir/{filename_noext}.aotdata: aot {filename}");
				ninja.WriteLine ($"  src_file={filename}");
				ninja.WriteLine ($"  outfile=$builddir/{filename}.s");
				ninja.WriteLine ($"  data_outfile=$builddir/{filename_noext}.aotdata");

				ninja.WriteLine ($"build $builddir/{filename}.o: assemble $builddir/{filename}.s");

				ninja.WriteLine ($"build $appdir/{filename_noext}.aotdata: cp {filename_noext}.aotdata");
			}

			ofiles += " " + ($"$builddir/{filename}.o");

			var aname = AssemblyName.GetAssemblyName (assembly);
			assembly_names.Add (aname.Name);
		}

		ninja.WriteLine ("build $appdir: mkdir");

		if (isdev) {
			ninja.WriteLine ($"build $appdir/{bundle_executable}: gen-exe {ofiles} $builddir/main.o $mono_sdkdir/ios-target64/lib/libmonosgen-2.0.a $monoios_dir/libmonoios.a");
			ninja.WriteLine ("build $builddir/main.o: compile-objc $builddir/main.m");
		} else {
			ninja.WriteLine ($"build $appdir/{bundle_executable}: cp $monoios_dir/test-runner");
		}
		ninja.WriteLine ("build $builddir/Info.plist.binary: plutil $builddir/Info.plist");
		ninja.WriteLine ("build $appdir/Info.plist: cpifdiff $builddir/Info.plist.binary");
		ninja.WriteLine ("build $appdir/config.json: cpifdiff $builddir/config.json");
		ninja.WriteLine ("build $builddir/Entitlements.xcent: cpifdiff $monoios_dir/Entitlements.xcent");
		if (isdev)
			ninja.WriteLine ($"build $appdir/_CodeSignature: codesign $appdir/{bundle_executable} | $builddir/Entitlements.xcent");
		else
			ninja.WriteLine ($"build $appdir/_CodeSignature: codesign-sim $appdir/{bundle_executable} | $builddir/Entitlements.xcent");
		ninja.WriteLine ("  entitlements=$builddir/Entitlements.xcent");
		ninja.WriteLine ("build $appdir/Base.lproj: cp-recursive $monoios_dir/Base.lproj");

		ninja.Close ();

		GenMain (builddir, assembly_names);
	}
}
