// 
// PcFileCache.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace Mono.PkgConfig
{
	internal interface IPcFileCacheContext
	{
		// In the implementation of this method, the host application can extract
		// information from the pc file and store it in the PackageInfo object
		void StoreCustomData (PcFile pcfile, PackageInfo pkg);
		
		// Should return false if the provided package does not have required
		// custom data
		bool IsCustomDataComplete (string pcfile, PackageInfo pkg);
		
		// Called to report errors
		void ReportError (string message, Exception ex);
	}
	
	internal class PcFileCache
	{
		const string CACHE_VERSION = "2";
		
		Dictionary<string, PackageInfo> infos = new Dictionary<string, PackageInfo> ();
		Dictionary<string, PackageAssemblyInfo> assemblyLocations;
		string cacheFile;
		bool hasChanges;
		IPcFileCacheContext ctx;
		
		public PcFileCache (IPcFileCacheContext ctx)
		{
			this.ctx = ctx;
			try {
				string path = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				path = Path.Combine (path, "xbuild");
				if (!Directory.Exists (path))
					Directory.CreateDirectory (path);
				cacheFile = Path.Combine (path, "pkgconfig-cache-" + CACHE_VERSION + ".xml");
				
				if (File.Exists (cacheFile))
					Load ();
				
			} catch (Exception ex) {
				ctx.ReportError ("pc file cache could not be loaded.", ex);
			}
		}
		
		// Updates the pkg-config index, using the default search directories
		public void Update ()
		{
			string pkgConfigPath = Environment.GetEnvironmentVariable ("PKG_CONFIG_PATH");
			string pkgConfigDir = Environment.GetEnvironmentVariable ("PKG_CONFIG_LIBDIR");
			Update (GetPkgconfigPaths (null, pkgConfigPath, pkgConfigDir));
		}

		// Updates the pkg-config index, looking for .pc files in the provided directories
		public void Update (IEnumerable<string> pkgConfigDirs)
		{
			foreach (string pcdir in pkgConfigDirs) {
				foreach (string pcfile in Directory.GetFiles (pcdir, "*.pc"))
					GetPackageInfo (pcfile);
			}
			Save ();
		}
		
		// Returns the location of an assembly, given the full name
		public PackageAssemblyInfo GetAssemblyLocation (string fullName)
		{
			lock (infos) {
				if (assemblyLocations == null) {
					// Populate on demand
					assemblyLocations = new Dictionary<string, PackageAssemblyInfo> ();
					foreach (PackageInfo info in infos.Values) {
						if (info.IsValidPackage) {
							foreach (PackageAssemblyInfo asm in info.Assemblies)
								assemblyLocations [NormalizeAsmName (asm.FullName)] = asm;
						}
					}
				}
			}
			// This collection is read-only once built, so there is no need for a lock
			PackageAssemblyInfo pasm;
			assemblyLocations.TryGetValue (NormalizeAsmName (fullName), out pasm);
			return pasm;
		}
		
		public IEnumerable<PackageAssemblyInfo> ResolveAssemblyName (string name)
		{
			foreach (PackageInfo pinfo in infos.Values) {
				if (pinfo.IsValidPackage) {
					foreach (PackageAssemblyInfo asm in pinfo.Assemblies) {
						if (asm.Name == name)
							yield return asm;
					}
				}
			}
		}
		
		// Returns information about a .pc file
		public PackageInfo GetPackageInfo (string file)
		{
			PackageInfo info;
			file = Path.GetFullPath (file);
			
			DateTime wtime = File.GetLastWriteTime (file);
			
			lock (infos) {
				if (infos.TryGetValue (file, out info)) {
					if (info.LastWriteTime == wtime)
						return info;
				}
			}

			try {
				info = ParsePackageInfo (file);
			} catch (Exception ex) {
				ctx.ReportError ("Error while parsing .pc file", ex);
				info = new PackageInfo ();
			}
			
			lock (infos) {
				if (!info.IsValidPackage)
					info = new PackageInfo (); // Create a default empty instance
				info.LastWriteTime = wtime;
				infos [file] = info;
				hasChanges = true;
			}
			
			return info;
		}
		
		FileStream OpenFile (FileAccess access)
		{
			int retries = 6;
			FileMode mode = access == FileAccess.Read ? FileMode.Open : FileMode.Create;
			Exception lastException = null;
			
			while (retries > 0) {
				try {
					return new FileStream (cacheFile, mode, access, FileShare.None);
				} catch (Exception ex) {
					// the file may be locked by another app. Wait a bit and try again
					lastException = ex;
					System.Threading.Thread.Sleep (200);
					retries--;
				}
			}
			ctx.ReportError ("File could not be opened: " + cacheFile, lastException);
			return null;
		}
		
		void Load ()
		{
			// The serializer can't be used because this file is reused in xbuild
			using (FileStream fs = OpenFile (FileAccess.Read)) {
				if (fs == null)
					return;
				XmlTextReader xr = new XmlTextReader (fs);
				xr.MoveToContent ();
				xr.ReadStartElement ();
				xr.MoveToContent ();
				
				while (xr.NodeType == XmlNodeType.Element)
					ReadPackage (xr);
			}
		}
		
		public void Save ()
		{
			// The serializer can't be used because this file is reused in xbuild
			lock (infos) {
				if (!hasChanges)
					return;
				
				using (FileStream fs = OpenFile (FileAccess.Write)) {
					if (fs == null)
						return;
					XmlTextWriter tw = new XmlTextWriter (new StreamWriter (fs));
					tw.Formatting = Formatting.Indented;
					
					tw.WriteStartElement ("PcFileCache");
					foreach (KeyValuePair<string,PackageInfo> file in infos) {
						WritePackage (tw, file.Key, file.Value);
					}
					tw.WriteEndElement (); // PcFileCache
					tw.Flush ();
					
					hasChanges = false;
				}
			}
		}
		
		void WritePackage (XmlTextWriter tw, string file, PackageInfo pinfo)
		{
			tw.WriteStartElement ("File");
			tw.WriteAttributeString ("path", file);
			tw.WriteAttributeString ("lastWriteTime", XmlConvert.ToString (pinfo.LastWriteTime, XmlDateTimeSerializationMode.Local));
			
			if (pinfo.IsValidPackage) {
				if (pinfo.Name != null)
					tw.WriteAttributeString ("name", pinfo.Name);
				if (pinfo.Version != null)
					tw.WriteAttributeString ("version", pinfo.Version);
				if (!string.IsNullOrEmpty (pinfo.Description))
					tw.WriteAttributeString ("description", pinfo.Description);
				if (!pinfo.IsGacPackage)
					tw.WriteAttributeString ("gacPackage", "false");
				if (pinfo.CustomData != null) {
					foreach (KeyValuePair<string,string> cd in pinfo.CustomData)
						tw.WriteAttributeString (cd.Key, cd.Value);
				}
				foreach (PackageAssemblyInfo asm in pinfo.Assemblies) {
					tw.WriteStartElement ("Assembly");
					tw.WriteAttributeString ("name", asm.Name);
					tw.WriteAttributeString ("version", asm.Version);
					tw.WriteAttributeString ("culture", asm.Culture);
					tw.WriteAttributeString ("publicKeyToken", asm.PublicKeyToken);
					tw.WriteAttributeString ("file", asm.File);
					tw.WriteEndElement (); // Assembly
				}
			}
			tw.WriteEndElement (); // File
		}
		
		void ReadPackage (XmlReader tr)
		{
			PackageInfo pinfo = new PackageInfo ();
			string file = null;
			
			tr.MoveToFirstAttribute ();
			do {
				switch (tr.LocalName) {
					case "path": file = tr.Value; break;
					case "lastWriteTime": pinfo.LastWriteTime = XmlConvert.ToDateTime (tr.Value, XmlDateTimeSerializationMode.Local); break;
					case "name": pinfo.Name = tr.Value; break;
					case "version": pinfo.Version = tr.Value; break;
					case "description": pinfo.Description = tr.Value; break;
					case "gacPackage": pinfo.IsGacPackage = tr.Value != "false"; break;
					default: pinfo.SetData (tr.LocalName, tr.Value); break;
				}
			} while (tr.MoveToNextAttribute ());
			
			tr.MoveToElement ();
			
			if (!tr.IsEmptyElement) {
				tr.ReadStartElement ();
				tr.MoveToContent ();
				while (tr.NodeType == XmlNodeType.Element) {
					PackageAssemblyInfo asm = new PackageAssemblyInfo ();
					asm.Name = tr.GetAttribute ("name");
					asm.Version = tr.GetAttribute ("version");
					asm.Culture = tr.GetAttribute ("culture");
					asm.PublicKeyToken = tr.GetAttribute ("publicKeyToken");
					asm.File = tr.GetAttribute ("file");
					if (pinfo.Assemblies == null)
						pinfo.Assemblies = new List<PackageAssemblyInfo> ();
					asm.ParentPackage = pinfo;
					pinfo.Assemblies.Add (asm);
					tr.Read ();
					tr.MoveToContent ();
				}
				tr.MoveToContent ();
				tr.ReadEndElement ();
			} else
				tr.Read ();
			tr.MoveToContent ();
			
			if (!pinfo.IsValidPackage || ctx.IsCustomDataComplete (file, pinfo))
				infos [file] = pinfo;
		}
		
		public object SyncRoot {
			get { return infos; }
		}
		
		
		PackageInfo ParsePackageInfo (string pcfile)
		{
			PackageInfo pinfo = new PackageInfo ();
			pinfo.Name = Path.GetFileNameWithoutExtension (pcfile);
			List<string> fullassemblies = null;
			bool gacPackageSet = false;
			
			PcFile file = new PcFile ();
			file.Load (pcfile);
			
			if (file.HasErrors)
				return pinfo;
			
			if (file.Libs != null && file.Libs.IndexOf (".dll") != -1) {
				if (file.Libs.IndexOf ("-lib:") != -1 || file.Libs.IndexOf ("/lib:") != -1) {
					fullassemblies = GetAssembliesWithLibInfo (file.Libs);
				} else {
					fullassemblies = GetAssembliesWithoutLibInfo (file.Libs);
				}
			}
			
			string value = file.GetVariable ("Libraries");
			if (!string.IsNullOrEmpty (value))
				fullassemblies = GetAssembliesFromLibrariesVar (value);
			
			pinfo.Version = file.Version;
			pinfo.Description = file.Description;

			value = file.GetVariable ("GacPackage");
			if (value != null) {
				value = value.ToLower ();
				pinfo.IsGacPackage = value == "yes" || value == "true";
				gacPackageSet = true;
			}
	
			if (fullassemblies == null)
				return pinfo;
			
			string pcDir = Path.GetDirectoryName (pcfile);
			string monoPrefix = Path.GetDirectoryName (Path.GetDirectoryName (pcDir));
			monoPrefix = Path.GetFullPath (monoPrefix + Path.DirectorySeparatorChar + "lib" + Path.DirectorySeparatorChar + "mono" + Path.DirectorySeparatorChar);

			List<PackageAssemblyInfo> list = new List<PackageAssemblyInfo> ();
			foreach (string assembly in fullassemblies) {
				string asm;
				if (Path.IsPathRooted (assembly))
					asm = Path.GetFullPath (assembly);
				else {
					if (Path.GetDirectoryName (assembly).Length == 0) {
						asm = assembly;
					} else {
						asm = Path.GetFullPath (Path.Combine (pcDir, assembly));
					}
				}
				if (File.Exists (asm)) {
					PackageAssemblyInfo pi = new PackageAssemblyInfo ();
					pi.File = asm;
					pi.ParentPackage = pinfo;
					pi.UpdateFromFile (pi.File);
					list.Add (pi);
					if (!gacPackageSet && !asm.StartsWith (monoPrefix) && Path.IsPathRooted (asm)) {
						// Assembly installed outside $(prefix)/lib/mono. It is most likely not a gac package.
						gacPackageSet = true;
						pinfo.IsGacPackage = false;
					}
				}
			}
			pinfo.Assemblies = list;
			ctx.StoreCustomData (file, pinfo);
			
			return pinfo;
		}
		
		private List<string> GetAssembliesWithLibInfo (string line)
		{
			List<string> references = new List<string> ();
			List<string> libdirs = new List<string> ();
			List<string> retval = new List<string> ();
			foreach (string piece in line.Split (' ')) {
				if (piece.ToLower ().Trim ().StartsWith ("/r:") || piece.ToLower ().Trim ().StartsWith ("-r:")) {
					references.Add (piece.Substring (3).Trim ());
				} else if (piece.ToLower ().Trim ().StartsWith ("/lib:") || piece.ToLower ().Trim ().StartsWith ("-lib:")) {
					libdirs.Add (piece.Substring (5).Trim ());
				}
			}
	
			foreach (string refrnc in references) {
				foreach (string libdir in libdirs) {
					if (File.Exists (libdir + Path.DirectorySeparatorChar + refrnc)) {
						retval.Add (libdir + Path.DirectorySeparatorChar + refrnc);
					}
				}
			}
	
			return retval;
		}
		
		List<string> GetAssembliesFromLibrariesVar (string line)
		{
			List<string> references = new List<string> ();
			foreach (string reference in line.Split (' ')) {
				if (!string.IsNullOrEmpty (reference))
					references.Add (reference);
			}
			return references;
		}
	
		private List<string> GetAssembliesWithoutLibInfo (string line)
		{
			List<string> references = new List<string> ();
			foreach (string reference in line.Split (' ')) {
				if (reference.ToLower ().Trim ().StartsWith ("/r:") || reference.ToLower ().Trim ().StartsWith ("-r:")) {
					string final_ref = reference.Substring (3).Trim ();
					references.Add (final_ref);
				}
			}
			return references;
		}
		
		public IEnumerable<string> GetPkgconfigPaths (string prefix, string pkgConfigPath, string pkgConfigLibdir)
		{
			char[] sep = new char[] { Path.PathSeparator };
			
			string[] pkgConfigPaths = null;
			if (!String.IsNullOrEmpty (pkgConfigPath)) {
				pkgConfigPaths = pkgConfigPath.Split (sep, StringSplitOptions.RemoveEmptyEntries);
				if (pkgConfigPaths.Length == 0)
					pkgConfigPaths = null;
			}
			
			string[] pkgConfigLibdirs = null;
			if (!String.IsNullOrEmpty (pkgConfigLibdir)) {
				pkgConfigLibdirs = pkgConfigLibdir.Split (sep, StringSplitOptions.RemoveEmptyEntries);
				if (pkgConfigLibdirs.Length == 0)
					pkgConfigLibdirs = null;
			}
			
			if (prefix == null)
				prefix = PathUp (typeof (int).Assembly.Location, 4);
			
			IEnumerable<string> paths = GetUnfilteredPkgConfigDirs (pkgConfigPaths, pkgConfigLibdirs, new string [] { prefix });
			return NormaliseAndFilterPaths (paths, Environment.CurrentDirectory);
		}
		
		IEnumerable<string> GetUnfilteredPkgConfigDirs (IEnumerable<string> pkgConfigPaths, IEnumerable<string> pkgConfigLibdirs, IEnumerable<string> systemPrefixes)
		{
			if (pkgConfigPaths != null) {
				foreach (string dir in pkgConfigPaths)
					yield return dir;
			}
			
			if (pkgConfigLibdirs != null) {
				foreach (string dir in pkgConfigLibdirs)
					yield return dir;
			} else if (systemPrefixes != null) {
				string[] suffixes = new string [] {
					Path.Combine ("lib", "pkgconfig"),
					Path.Combine ("lib64", "pkgconfig"),
					Path.Combine ("libdata", "pkgconfig"),
					Path.Combine ("share", "pkgconfig"),
				};
				foreach (string prefix in systemPrefixes)
					foreach (string suffix in suffixes)
						yield return Path.Combine (prefix, suffix);
			}
		}
		
		IEnumerable<string> NormaliseAndFilterPaths (IEnumerable<string> paths, string workingDirectory)
		{
			HashSet<string> filtered = new HashSet<string> ();
			foreach (string p in paths) {
				string path = p;
				if (!Path.IsPathRooted (path))
					path = Path.Combine (workingDirectory, path);
				path = Path.GetFullPath (path);
				if (!filtered.Add (path))
					continue;
				try {
					if (!Directory.Exists (path))
						continue;
				} catch (IOException ex) {
					ctx.ReportError ("Error checking for directory '" + path + "'.", ex);
				}
				yield return path;
			}
		}
		
		static string PathUp (string path, int up)
		{
			if (up == 0)
				return path;
			for (int i = path.Length -1; i >= 0; i--) {
				if (path[i] == Path.DirectorySeparatorChar) {
					up--;
					if (up == 0)
						return path.Substring (0, i);
				}
			}
			return null;
		}
		
		public static string NormalizeAsmName (string name)
		{
			int i = name.ToLower ().IndexOf (", publickeytoken=null");
			if (i != -1)
				name = name.Substring (0, i).Trim ();
			i = name.ToLower ().IndexOf (", processorarchitecture=");
			if (i != -1)
				name = name.Substring (0, i).Trim ();
			return name;
		}
	}

	internal class PcFile
	{
		Dictionary<string,string> variables = new Dictionary<string, string> ();
		
		public string FilePath { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Version { get; set; }
		public string Libs { get; set; }
		public bool HasErrors { get; set; }
		
		public string GetVariable (string varName)
		{
			string val;
			variables.TryGetValue (varName, out val);
			return val;
		}
		
		public void Load (string pcfile)
		{
			FilePath = pcfile;
			variables.Add ("pcfiledir", Path.GetDirectoryName (pcfile));
			using (StreamReader reader = new StreamReader (pcfile)) {
				string line;
				while ((line = reader.ReadLine ()) != null) {
					int i = line.IndexOf (':');
					int j = line.IndexOf ('=');
					int k = System.Math.Min (i != -1 ? i : int.MaxValue, j != -1 ? j : int.MaxValue);
					if (k == int.MaxValue)
						continue;
					string var = line.Substring (0, k).Trim ();
					string value = line.Substring (k + 1).Trim ();
					value = Evaluate (value);
					
					if (k == j) {
						// Is variable
						variables [var] = value;
					}
					else {
						switch (var) {
							case "Name": Name = value; break;
							case "Description": Description = value; break;
							case "Version": Version = value; break;
							case "Libs": Libs = value; break;
						}
					}
				}
			}
		}
		
		string Evaluate (string value)
		{
			int i = value.IndexOf ("${");
			if (i == -1)
				return value;

			StringBuilder sb = new StringBuilder ();
			int last = 0;
			while (i != -1 && i < value.Length) {
				sb.Append (value.Substring (last, i - last));
				if (i == 0 || value [i - 1] != '$') {
					// Evaluate if var is not escaped
					i += 2;
					int n = value.IndexOf ('}', i);
					if (n == -1 || n == i) {
						// Closing bracket not found or empty name
						HasErrors = true;
						return value;
					}
					string rname = value.Substring (i, n - i);
					string rval;
					if (variables.TryGetValue (rname, out rval))
						sb.Append (rval);
					else {
						HasErrors = true;
						return value;
					}
					i = n + 1;
					last = i;
				} else
					last = i++;
				
				if (i < value.Length - 1)
					i = value.IndexOf ("${", i);
			}
			sb.Append (value.Substring (last, value.Length - last));
			return sb.ToString ();
		}
	}
	
	internal class PackageInfo
	{
		Dictionary<string,string> customData;
		
		public PackageInfo ()
		{
			IsGacPackage = true;
		}

		public string Name { get; set; }
		
		public bool IsGacPackage { get; set; }
		
		public string Version { get; set; }
		
		public string Description { get; set; }
		
		internal List<PackageAssemblyInfo> Assemblies { get; set; }
		
		public string GetData (string name)
		{
			if (customData == null)
				return null;
			string res;
			customData.TryGetValue (name, out res);
			return res;
		}
		
		public void SetData (string name, string value)
		{
			if (customData == null)
				customData = new Dictionary<string, string> ();
			customData [name] = value;
		}
		
		internal Dictionary<string,string> CustomData {
			get { return customData; }
		}
		
		internal DateTime LastWriteTime { get; set; }
		
		internal bool IsValidPackage {
			get { return Assemblies != null && Assemblies.Count > 0; }
		}
		
		internal bool HasCustomData {
			get { return customData != null && customData.Count > 0; }
		}
	}
	
	class PackageAssemblyInfo
	{
		public string File { get; set; }
		
		public string Name;
		
		public string Version;
		
		public string Culture;
		
		public string PublicKeyToken;
		
		public string FullName {
			get {
				string fn = Name + ", Version=" + Version;
				if (!string.IsNullOrEmpty (Culture))
					fn += ", Culture=" + Culture;
				if (!string.IsNullOrEmpty (PublicKeyToken))
					fn += ", PublicKeyToken=" + PublicKeyToken;
				return fn;
			}
		}
		
		public PackageInfo ParentPackage { get; set; }
		
		public void UpdateFromFile (string file)
		{
			Update (System.Reflection.AssemblyName.GetAssemblyName (file));
		}
		
		public void Update (System.Reflection.AssemblyName aname)
		{
			Name = aname.Name;
			Version = aname.Version.ToString ();
			if (aname.CultureInfo != null) {
				if (aname.CultureInfo.LCID == System.Globalization.CultureInfo.InvariantCulture.LCID)
					Culture = "neutral";
				else
					Culture = aname.CultureInfo.Name;
			}
			string fn = aname.ToString ();
			string key = "publickeytoken=";
			int i = fn.ToLower().IndexOf (key) + key.Length;
			int j = fn.IndexOf (',', i);
			if (j == -1) j = fn.Length;
			PublicKeyToken = fn.Substring (i, j - i);
		}
	}
}
