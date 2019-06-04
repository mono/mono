using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

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

	// IGNORED_ASSEMBLY
	internal static Dictionary<string, NuGetData> ignoredAsmTable = new Dictionary<string, NuGetData> ();

	// IGNORED_ASM_VER
	internal static SortedDictionary<string, NuGetData> ignoredAsmVerTable = new SortedDictionary<string, NuGetData> ();

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
			entries = zip.Entries.Where (e => e.FullName.StartsWith (prefix) && !e.FullName.Contains ("/ref/") && e.Name.EndsWith (".dll") && BadAssembliesToEnumTable.ContainsKey (e.Name));
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

	static void DumpOutput ()
	{
		if (dump_asm) {
			var sb = new StringBuilder();
			// sorted just by hashcode
			foreach (var data in ignoredAsmTable.Values.OrderBy (data => data.HashCode)) {
				sb.AppendLine ($"IGNORED_ASSEMBLY (0x{data.HashCode}, {data.EnumName}, \"{data.ModuleVersionId}\", \"{data.NuGetVersionAndFramework}\"),");
			}

			if (sb.Length > 2)
				sb.Length = sb.Length - 2;
			Console.WriteLine (sb.ToString ());
		}

		//IGNORED_ASM_VER (SYS_IO_COMPRESSION, 4, 1, 2, 0),
		if (dump_ver) {
			// the default key (IgnoredAsmVerKey) is the order which we need for this
			var sb = new StringBuilder();
			foreach (var key in ignoredAsmVerTable.Keys) {
				var data = ignoredAsmVerTable [key];
				var ver = data.AssemblyVersion;
				sb.AppendLine ($"IGNORED_ASM_VER ({data.EnumName}, {ver.Major}, {ver.Minor}, {ver.Build}, {ver.Revision}),");
			}

			if (sb.Length > 2)
				sb.Length = sb.Length - 2;
			Console.WriteLine (sb.ToString ());
		}

		if (dump_guids_for_msbuild) {
			// This needs to be kept in sync with FilterDeniedAssemblies msbuild task in msbuild
			// Sory by assembly name and version
			var query = ignoredAsmTable.Values
						.GroupBy (data => data.AssemblyName)
						.OrderBy (group => group.Key)
						.Select (group => new { AssemblyName = group.Key, NuGets = group.OrderBy (data => data.AssemblyVersion).ThenBy (data => data.ModuleVersionId) });

			foreach (var g in query) {
				foreach (var data in g.NuGets) {
					var ver = data.AssemblyVersion;
					Console.WriteLine ($"denied:{data.AssemblyName},{data.ModuleVersionId},{ver.Major},{ver.Minor},{ver.Build},{ver.Revision}");
				}
			}
		}
	}

	static bool dump_asm, dump_ver, dump_guids_for_msbuild;
	static void Main (string[] args) {

		if (args.Length > 1) {
			dump_asm = args [1].Equals ("asm");
			dump_ver = args [1].Equals ("ver");
			dump_guids_for_msbuild = args [1].Equals ("guids_for_msbuild");

			if (args [1].Equals("all"))
				dump_asm = dump_ver = dump_guids_for_msbuild = true;
		} else {
			dump_asm = true;
		}
		foreach (var f in Directory.GetFiles (args [0], "*.nupkg").OrderBy (nupkg => Path.GetFileName (nupkg))) {
			DumpNuget (f);
		}

		DumpOutput ();
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
		var nugetData = p.ParseAssembly (data, version, entry.Name, entry.FullName);

		if (!ignoredAsmTable.ContainsKey (nugetData.IgnoredAsmKey))
			ignoredAsmTable [nugetData.IgnoredAsmKey] = nugetData;

		if (!ignoredAsmVerTable.ContainsKey (nugetData.IgnoredAsmVerKey))
			ignoredAsmVerTable [nugetData.IgnoredAsmVerKey] = nugetData;

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

	public NuGetData ParseAssembly (byte[] data, string version, string name, string fullname) {
		var a = Assembly.ReflectionOnlyLoad (data);
		var m = a.GetModules ()[0];
		var id = m.ModuleVersionId.ToString ().ToUpper ();

		return new NuGetData {
			AssemblyName = name,
			EnumName = FileToEnum (name),
			NuGetVersionAndFramework = version + " " + FileToMoniker (fullname),
			AssemblyVersion = a.GetName ().Version,
			HashCode = Hash (id).ToString ("X"),
			ModuleVersionId = id
		};
	}
}

[Serializable]
class NuGetData
{
	public string AssemblyName;
	public string EnumName;
	public string NuGetVersionAndFramework;
	public Version AssemblyVersion;
	public string HashCode;
	public string ModuleVersionId;

	public string IgnoredAsmKey => $"{HashCode},{EnumName},{ModuleVersionId}";
	public string IgnoredAsmVerKey => $"{EnumName},{AssemblyVersion.ToString()}";
}
