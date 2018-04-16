using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Reflection;

class Driver {

	internal static Dictionary<string, string> BadAssembliesToEnumTable = new Dictionary<string, string> {
		{"System.Runtime.InteropServices.RuntimeInformation.dll", "SYS_RT_INTEROP_RUNTIME_INFO"},
		{"System.Globalization.Extensions.dll", "SYS_GLOBALIZATION_EXT"},
		{"System.IO.Compression.dll", "SYS_IO_COMPRESSION"},
		{"System.Net.Http.dll", "SYS_NET_HTTP"},
		{"System.Text.Encoding.CodePages.dll", "SYS_TEXT_ENC_CODEPAGES"},
		{"System.Reflection.DispatchProxy.dll", "SYS_REF_DISP_PROXY"},
		{"System.Threading.Overlapped.dll", "SYS_THREADING_OVERLAPPED"}
	};

	static ZipArchiveEntry FindSpecFile (ZipArchive zip) {
		foreach (var entry in zip.Entries) {
			if (entry.Name.EndsWith (".nuspec"))
				return entry;
		}
		throw new Exception ("Could not find nuspec file");
	}

	static void DumpNuget (string nupkg) {
		var zip = new ZipArchive(new FileStream (nupkg, FileMode.Open));

		var nuspec = FindSpecFile (zip);
		var l = XElement.Load (new StreamReader (nuspec.Open ()));
		var version = (from el in l.Descendants() where el.Name.LocalName == "version" select el.Value).FirstOrDefault ();

		var prefixesToCheckInOrder = new string[] {
			"lib/net4",
			"lib/netstandard1",
			"msbuildExtensions/Microsoft/Microsoft.NET.Build.Extensions/net4"
		};

		//
		// Logic copied from https://github.com/NuGet/NuGet.Client/blob/4cccb13833ad29d6a0bcff055460d964f1b49cfe/src/NuGet.Core/NuGet.Frameworks/DefaultFrameworkMappings.cs#L385
		//
		IEnumerable<ZipArchiveEntry> entries = null;
		foreach (var prefix in prefixesToCheckInOrder) {
			entries = zip.Entries.Where (e => e.FullName.StartsWith (prefix) && e.Name.EndsWith (".dll") && BadAssembliesToEnumTable.ContainsKey (e.Name));
			if (entries.Any ())
				break;
		}

		if (!entries.Any ()) {
			Console.Error.WriteLine ($"** Warning: No relevant assemblies found for nukpkg: {nupkg}");
			return;
		}

		// Only take assemblies from first prefix
		foreach (var et in entries) {
			LoadAndDump (et, version);
		}
	}

	static bool dump_asm, dump_ver, dump_guids_for_msbuild;
	static void Main (string[] args) {

		if (args.Length > 1) {
			dump_asm = args [1].Equals ("asm");
			dump_ver = args [1].Equals ("ver");
			dump_guids_for_msbuild = args [1].Equals ("guids_for_msbuild");
		} else {
			dump_asm = true;
		}
		foreach (var f in Directory.GetFiles (args [0], "*.nupkg")) {
			DumpNuget (f);
		}
	}

	static byte[] StreamToArray (Stream s) {
		using(var ms = new MemoryStream ()) {
			s.CopyTo (ms);
			return ms.ToArray ();
		}
	}

	static int domain_id = 1;
	static void LoadAndDump (ZipArchiveEntry entry, string version) {
		var data = StreamToArray (entry.Open ());
		AppDomain ad = AppDomain.CreateDomain ("parse_" + ++domain_id);
		DoParse p = (DoParse)ad.CreateInstanceAndUnwrap (typeof (DoParse).Assembly.FullName, typeof (DoParse).FullName);
		p.ParseAssembly (data, version, entry.Name, entry.FullName, dump_asm, dump_ver, dump_guids_for_msbuild);
		AppDomain.Unload (ad);
	}
}

class DoParse : MarshalByRefObject {

	static int Hash (string str) {
		int h = 5381;
        for (int i = 0;  i < str.Length; ++i)
            h = ((h << 5) + h) ^ str[i];
		return h;
	}

	static string FileToMoniker (string path) =>
					path.Split (Path.DirectorySeparatorChar)
						.Where (p => p.StartsWith ("net"))
						.FirstOrDefault ()
					?? "unknown";

	static string FileToEnum (string name) =>
					Driver.BadAssembliesToEnumTable.ContainsKey (name)
						? Driver.BadAssembliesToEnumTable [name]
						: throw new Exception ($"No idea what to do with {name}");

	public void ParseAssembly (byte[] data, string version, string name, string fullname, bool dump_asm, bool dump_ver, bool dump_guids_for_msbuild) {
		var a = Assembly.ReflectionOnlyLoad (data);
		var m = a.GetModules ()[0];
		var id = m.ModuleVersionId.ToString ().ToUpper ();
		var hash_code = Hash (id).ToString ("X");
		var str = FileToEnum (name);

		string ver_str = version + " " + FileToMoniker (fullname);

		if (dump_asm)
			Console.WriteLine ($"IGNORED_ASSEMBLY (0x{hash_code}, {str}, \"{id}\", \"{ver_str}\"),");

		//IGNORED_ASM_VER (SYS_IO_COMPRESSION, 4, 1, 2, 0),
		var ver = a.GetName ().Version;
		if (dump_ver) {
			Console.WriteLine ($"IGNORED_ASM_VER ({str}, {ver.Major}, {ver.Minor}, {ver.Build}, {ver.Revision}),");
		} else if (dump_guids_for_msbuild) {
			// This needs to be kept in sync with FilterDeniedAssemblies msbuild task in msbuild
			Console.WriteLine ($"{name},{id},{ver.Major},{ver.Minor},{ver.Build},{ver.Revision}");
		}
		
	}
}
