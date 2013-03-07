// 
// PcFileCacheAssembly.cs
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
	internal class LibraryPcFileCache: PcFileCache<LibraryPackageInfo>
	{
		Dictionary<string, PackageAssemblyInfo> assemblyLocations;
		
		public LibraryPcFileCache (IPcFileCacheContext<LibraryPackageInfo> ctx): base (ctx)
		{
		}
		
		protected override string CacheDirectory {
			get {
				string path = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
				path = Path.Combine (path, "xbuild");
				return path;
			}
		}
		
		// Returns the location of an assembly, given the full name
		public PackageAssemblyInfo GetAssemblyLocation (string fullName)
		{
			return GetAssemblyLocation (fullName, null);
		}
		
		public PackageAssemblyInfo GetAssemblyLocation (string fullName, IEnumerable<string> searchPaths)
		{
			lock (SyncRoot) {
				if (assemblyLocations == null) {
					// Populate on demand
					assemblyLocations = new Dictionary<string, PackageAssemblyInfo> ();
					foreach (LibraryPackageInfo info in GetPackages (searchPaths)) {
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
			return ResolveAssemblyName (name, null);
		}
		
		public IEnumerable<PackageAssemblyInfo> ResolveAssemblyName (string name, IEnumerable<string> searchPaths)
		{
			foreach (LibraryPackageInfo pinfo in GetPackages (searchPaths)) {
				if (pinfo.IsValidPackage) {
					foreach (PackageAssemblyInfo asm in pinfo.Assemblies) {
						if (asm.Name == name)
							yield return asm;
					}
				}
			}
		}
		
		protected override void WritePackageContent (XmlTextWriter tw, string file, LibraryPackageInfo pinfo)
		{
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
		
		protected override void ReadPackageContent (XmlReader tr, LibraryPackageInfo pinfo)
		{
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
		}
		
		protected override void ParsePackageInfo (PcFile file, LibraryPackageInfo pinfo)
		{
			List<string> fullassemblies = null;
			bool gacPackageSet = false;
			
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
			
			value = file.GetVariable ("GacPackage");
			if (value != null) {
				pinfo.IsGacPackage = 
					string.Equals (value, "yes", StringComparison.OrdinalIgnoreCase) ||
					string.Equals (value, "true", StringComparison.OrdinalIgnoreCase);
				gacPackageSet = true;
			}
	
			if (fullassemblies == null)
				return;
			
			string pcDir = Path.GetDirectoryName (file.FilePath);
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
		}
		
		private List<string> GetAssembliesWithLibInfo (string line)
		{
			List<string> references = new List<string> ();
			List<string> libdirs = new List<string> ();
			List<string> retval = new List<string> ();
			foreach (string piece in line.Split (' ')) {
				if (IsReferenceParameter (piece)) {
					references.Add (piece.Substring (3).Trim ());
				} else if (piece.TrimStart ().StartsWith ("/lib:", StringComparison.OrdinalIgnoreCase) ||
						piece.TrimStart ().StartsWith ("-lib:", StringComparison.OrdinalIgnoreCase)) {
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

		static bool IsReferenceParameter (string value)
		{
			return value.TrimStart ().StartsWith ("/r:", StringComparison.OrdinalIgnoreCase) ||
				value.TrimStart ().StartsWith ("-r:", StringComparison.OrdinalIgnoreCase);
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
				if (IsReferenceParameter (reference)) {
					string final_ref = reference.Substring (3).Trim ();
					references.Add (final_ref);
				}
			}
			return references;
		}
		
		public static string NormalizeAsmName (string name)
		{
			int i = name.IndexOf (", publickeytoken=null", StringComparison.OrdinalIgnoreCase);
			if (i != -1)
				name = name.Substring (0, i).Trim ();
			i = name.IndexOf (", processorarchitecture=", StringComparison.OrdinalIgnoreCase);
			if (i != -1)
				name = name.Substring (0, i).Trim ();
			return name;
		}
	}
	
	internal class LibraryPackageInfo: PackageInfo
	{
		public bool IsGacPackage {
			get { return GetData ("gacPackage") != "false"; }
			set {
				if (value)
					RemoveData ("gacPackage");
				else
					SetData ("gacPackage", "false");
			}
		}
		
		internal List<PackageAssemblyInfo> Assemblies { get; set; }
		
		internal protected override bool IsValidPackage {
			get { return Assemblies != null && Assemblies.Count > 0; }
		}
	}
	
	internal class PackageAssemblyInfo
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
		
		public LibraryPackageInfo ParentPackage { get; set; }
		
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
			int i = fn.IndexOf (key, StringComparison.OrdinalIgnoreCase) + key.Length;
			int j = fn.IndexOf (',', i);
			if (j == -1) j = fn.Length;
			PublicKeyToken = fn.Substring (i, j - i);
		}
	}
}
