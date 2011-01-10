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

// IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
// This code is shared with xbuild, which has to build with .NET 2.0,
// so no c# 3.0 syntax is allowed here.

namespace Mono.PkgConfig
{
	internal interface IPcFileCacheContext<TP> where TP:PackageInfo, new()
	{
		// In the implementation of this method, the host application can extract
		// information from the pc file and store it in the PackageInfo object
		void StoreCustomData (PcFile pcfile, TP pkg);
		
		// Should return false if the provided package does not have required
		// custom data
		bool IsCustomDataComplete (string pcfile, TP pkg);
		
		// Called to report errors
		void ReportError (string message, Exception ex);
	}
	
	internal interface IPcFileCacheContext: IPcFileCacheContext<PackageInfo>
	{
	}
	
	internal abstract class PcFileCache: PcFileCache<PackageInfo>
	{
		public PcFileCache (IPcFileCacheContext ctx): base (ctx)
		{
		}
	}
	
	internal abstract class PcFileCache<TP> where TP:PackageInfo, new()
	{
		const string CACHE_VERSION = "2";
		const string MacOSXExternalPkgConfigDir = "/Library/Frameworks/Mono.framework/External/pkgconfig";
		
		Dictionary<string, TP> infos = new Dictionary<string, TP> ();
		Dictionary<string, List<TP>> filesByFolder = new Dictionary<string, List<TP>> ();
		
		string cacheFile;
		bool hasChanges;
		IPcFileCacheContext<TP> ctx;
		IEnumerable<string> defaultPaths;
		
		public PcFileCache (IPcFileCacheContext<TP> ctx)
		{
			this.ctx = ctx;
			try {
				string path = CacheDirectory;
				if (!Directory.Exists (path))
					Directory.CreateDirectory (path);
				cacheFile = Path.Combine (path, "pkgconfig-cache-" + CACHE_VERSION + ".xml");
				
				if (File.Exists (cacheFile))
					Load ();
				
			} catch (Exception ex) {
				ctx.ReportError ("pc file cache could not be loaded.", ex);
			}
		}
		
		protected abstract string CacheDirectory { get; }
		
		// Updates the pkg-config index, using the default search directories
		public void Update ()
		{
			Update (GetDefaultPaths ());
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
		
		public IEnumerable<TP> GetPackages ()
		{
			return GetPackages (null);
		}
		
		public IEnumerable<TP> GetPackages (IEnumerable<string> pkgConfigDirs)
		{
			if (pkgConfigDirs == null)
				pkgConfigDirs = GetDefaultPaths ();

			foreach (string sp in pkgConfigDirs) {
				List<TP> list;
				if (filesByFolder.TryGetValue (Path.GetFullPath (sp), out list)) {
					foreach (TP p in list)
						yield return p;
				}
			}
		}
		
		public TP GetPackageInfoByName (string name)
		{
			return GetPackageInfoByName (name, null);
		}
		
		public TP GetPackageInfoByName (string name, IEnumerable<string> pkgConfigDirs)
		{
			foreach (TP p in GetPackages (pkgConfigDirs))
				if (p.Name == name)
					return p;
			return null;
		}
		
		// Returns information about a .pc file
		public TP GetPackageInfo (string file)
		{
			TP info, oldInfo = null;
			file = Path.GetFullPath (file);
			
			DateTime wtime = File.GetLastWriteTime (file);
			
			lock (infos) {
				if (infos.TryGetValue (file, out info)) {
					if (info.LastWriteTime == wtime)
						return info;
					oldInfo = info;
				}
			}

			try {
				info = ParsePackageInfo (file);
			} catch (Exception ex) {
				ctx.ReportError ("Error while parsing .pc file: " + file, ex);
				info = new TP ();
			}
			
			lock (infos) {
				if (!info.IsValidPackage)
					info = new TP (); // Create a default empty instance
				info.LastWriteTime = wtime;
				Add (file, info, oldInfo);
				hasChanges = true;
			}
			
			return info;
		}
		
		void Add (string file, TP info, TP replacedInfo)
		{
			infos [file] = info;
			string dir = Path.GetFullPath (Path.GetDirectoryName (file));
			List<TP> list;
			if (!filesByFolder.TryGetValue (dir, out list)) {
				list = new List<TP> ();
				filesByFolder [dir] = list;
			}
			if (replacedInfo != null) {
				int i = list.IndexOf (replacedInfo);
				if (i != -1) {
					list [i] = info;
					return;
				}
			}
			list.Add (info);
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
					foreach (KeyValuePair<string,TP> file in infos) {
						WritePackage (tw, file.Key, file.Value);
					}
					tw.WriteEndElement (); // PcFileCache
					tw.Flush ();
					
					hasChanges = false;
				}
			}
		}
		
		void WritePackage (XmlTextWriter tw, string file, TP pinfo)
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
				if (!string.IsNullOrEmpty (pinfo.Requires))
					tw.WriteAttributeString ("requires", pinfo.Requires);
				if (pinfo.CustomData != null) {
					foreach (KeyValuePair<string,string> cd in pinfo.CustomData)
						tw.WriteAttributeString (cd.Key, cd.Value);
				}
				WritePackageContent (tw, file, pinfo);
			}
			tw.WriteEndElement (); // File
		}
		
		protected virtual void WritePackageContent (XmlTextWriter tw, string file, TP pinfo)
		{
		}
		
		void ReadPackage (XmlReader tr)
		{
			TP pinfo = new TP ();
			string file = null;
			
			tr.MoveToFirstAttribute ();
			do {
				switch (tr.LocalName) {
					case "path": file = tr.Value; break;
					case "lastWriteTime": pinfo.LastWriteTime = XmlConvert.ToDateTime (tr.Value, XmlDateTimeSerializationMode.Local); break;
					case "name": pinfo.Name = tr.Value; break;
					case "version": pinfo.Version = tr.Value; break;
					case "description": pinfo.Description = tr.Value; break;
					case "requires": pinfo.Requires = tr.Value; break;
					default: pinfo.SetData (tr.LocalName, tr.Value); break;
				}
			} while (tr.MoveToNextAttribute ());
			
			tr.MoveToElement ();
			
			if (!tr.IsEmptyElement) {
				tr.ReadStartElement ();
				tr.MoveToContent ();
				ReadPackageContent (tr, pinfo);
				tr.MoveToContent ();
				tr.ReadEndElement ();
			} else
				tr.Read ();
			tr.MoveToContent ();
			
			if (!pinfo.IsValidPackage || ctx.IsCustomDataComplete (file, pinfo))
				Add (file, pinfo, null);
		}
		
		protected virtual void ReadPackageContent (XmlReader tr, TP pinfo)
		{
		}
		
		public object SyncRoot {
			get { return infos; }
		}
		
		
		TP ParsePackageInfo (string pcfile)
		{
			PcFile file = new PcFile ();
			file.Load (pcfile);
			
			TP pinfo = new TP ();
			pinfo.Name = Path.GetFileNameWithoutExtension (file.FilePath);
			
			if (!file.HasErrors) {
				pinfo.Version = file.Version;
				pinfo.Description = file.Description;
				pinfo.Requires = file.Requires;
				ParsePackageInfo (file, pinfo);
				if (pinfo.IsValidPackage)
					ctx.StoreCustomData (file, pinfo);
			}
			return pinfo;
		}
		
		protected virtual void ParsePackageInfo (PcFile file, TP pinfo)
		{
		}
		
		IEnumerable<string> GetDefaultPaths ()
		{
			if (defaultPaths == null) {
				// For mac osx, look in the 'External' dir on macosx,
				// see bug #663180
				string pkgConfigPath = String.Format ("{0}:{1}",
						Microsoft.Build.Tasks.Utilities.RunningOnMac ? MacOSXExternalPkgConfigDir : String.Empty,
						Environment.GetEnvironmentVariable ("PKG_CONFIG_PATH") ?? String.Empty);

				string pkgConfigDir = Environment.GetEnvironmentVariable ("PKG_CONFIG_LIBDIR");
				defaultPaths = GetPkgconfigPaths (null, pkgConfigPath, pkgConfigDir);
			}
			return defaultPaths;
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
					//FIXME: is this the correct order? share should be before lib but not sure about others.
					Path.Combine ("share", "pkgconfig"),
					Path.Combine ("lib", "pkgconfig"),
					Path.Combine ("lib64", "pkgconfig"),
					Path.Combine ("libdata", "pkgconfig"),
				};
				foreach (string prefix in systemPrefixes)
					foreach (string suffix in suffixes)
						yield return Path.Combine (prefix, suffix);
			}
		}
		
		IEnumerable<string> NormaliseAndFilterPaths (IEnumerable<string> paths, string workingDirectory)
		{
			Dictionary<string,string> filtered = new Dictionary<string,string> ();
			foreach (string p in paths) {
				string path = p;
				if (!Path.IsPathRooted (path))
					path = Path.Combine (workingDirectory, path);
				path = Path.GetFullPath (path);
				if (filtered.ContainsKey (path))
					continue;
				filtered.Add (path,path);
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
	}

	internal class PcFile
	{
		Dictionary<string,string> variables = new Dictionary<string, string> ();
		
		string description;
		public string Description {
			get { return description; }
			set { description = value; }
		}
		
		string filePath;
		public string FilePath {
			get { return filePath; }
			set { filePath = value; }
		}
		
		bool hasErrors;
		public bool HasErrors {
			get { return hasErrors; }
			set { hasErrors = value; }
		}
		
		string libs;
		public string Libs {
			get { return libs; }
			set { libs = value; }
		}
		
		string name;
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		string version;
		public string Version {
			get { return version; }
			set { version = value; }
		}
		
		string requires;
		public string Requires {
			get { return requires; }
			set { requires = value; }
		}
		
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
							case "Requires": Requires = value; break;
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
				
				if (i < value.Length)
					i = value.IndexOf ("${", i);
			}
			sb.Append (value.Substring (last, value.Length - last));
			return sb.ToString ();
		}
	}
	
	internal class PackageInfo
	{
		Dictionary<string,string> customData;
		DateTime lastWriteTime;
		
		string name;
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		string version;
		public string Version {
			get { return version; }
			set { version = value; }
		}
		
		string description;
		public string Description {
			get { return description; }
			set { description = value; }
		}
		
		string requires;
		public string Requires {
			get { return requires; }
			set { requires = value; }
		}
		
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
		
		public void RemoveData (string name)
		{
			if (customData != null)
				customData.Remove (name);
		}
		
		internal Dictionary<string,string> CustomData {
			get { return customData; }
		}
		
		internal DateTime LastWriteTime {
			get { return lastWriteTime; }
			set { lastWriteTime = value; }
		}
		
		internal bool HasCustomData {
			get { return customData != null && customData.Count > 0; }
		}
		
		internal protected virtual bool IsValidPackage {
			get { return HasCustomData; }
		}
	}
}
