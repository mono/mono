using System;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Reflection;

class Driver {
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

		foreach (var et in from e in zip.Entries where (e.FullName.StartsWith ("lib/net4") || e.FullName.StartsWith ("lib/netcore")) && e.Name.EndsWith (".dll") select e) {
			LoadAndDump (et, version);
		}
	}
	static void Main (string[] args) {
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
		// Console.WriteLine ("Dumping {0}", entry);
		var data = StreamToArray (entry.Open ());
		AppDomain ad = AppDomain.CreateDomain ("parse_" + ++domain_id);
		DoParse p = (DoParse)ad.CreateInstanceAndUnwrap (typeof (DoParse).Assembly.FullName, typeof (DoParse).FullName);
		p.ParseAssembly (data, version, entry.Name, entry.FullName);
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
	static string FileToEnum (string name) {
		switch (name) {
		case "System.Runtime.InteropServices.RuntimeInformation.dll": return "SYS_RT_INTEROP_RUNTIME_INFO";
		case "System.Globalization.Extensions.dll": return "SYS_GLOBALIZATION_EXT";
		case "System.IO.Compression.dll": return "SYS_IO_COMPRESSION";
		case "System.Net.Http.dll": return "SYS_NET_HTTP";
		case "System.Text.Encoding.CodePages.dll": return "SYS_TEXT_ENC_CODEPAGES";
		default: throw new Exception ($"No idea what to do with {name}");
		}
	}

	static string FileToMoniker (string p) {
		var parts = p.Split (Path.DirectorySeparatorChar);
		return parts[parts.Length - 2];
	}

	public void ParseAssembly (byte[] data, string version, string name, string fullname) {
		var a = Assembly.ReflectionOnlyLoad (data);
		var m = a.GetModules ()[0];
		var id = m.ModuleVersionId.ToString ().ToUpper ();
		var hash_code = Hash (id).ToString ("X");
		var str = FileToEnum (name);

		string ver_str = version + " " + FileToMoniker (fullname);	
		Console.WriteLine ($"IGNORED_ASSEMBLY (0x{hash_code}, {str}, \"{id}\", \"{ver_str}\"),");
	}
}