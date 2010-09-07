//
// System.Web.Compilation.BuildManagerDirectoryBuilder
//
// Authors:
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2008-2009 Novell, Inc (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Util;

namespace System.Web.Compilation
{
	sealed class BuildManagerDirectoryBuilder
	{
		sealed class BuildProviderItem
		{
			public BuildProvider Provider;
			public int ListIndex;
			public int ParentIndex;
			
			public BuildProviderItem (BuildProvider bp, int listIndex, int parentIndex)
			{
				this.Provider = bp;
				this.ListIndex = listIndex;
				this.ParentIndex = parentIndex;
			}
		}
		
		readonly VirtualPath virtualPath;
		readonly string virtualPathDirectory;
		CompilationSection compilationSection;
		Dictionary <string, BuildProvider> buildProviders;
		VirtualPathProvider vpp;
		
		CompilationSection CompilationSection {
			get {
				if (compilationSection == null)
					compilationSection = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
				return compilationSection;
			}
		}
		
		public BuildManagerDirectoryBuilder (VirtualPath virtualPath)
		{
			if (virtualPath == null)
				throw new ArgumentNullException ("virtualPath");

			this.vpp = HostingEnvironment.VirtualPathProvider;
			this.virtualPath = virtualPath;
			this.virtualPathDirectory = VirtualPathUtility.GetDirectory (virtualPath.Absolute);
		}

		public List <BuildProviderGroup> Build (bool single)
		{
			if (StrUtils.StartsWith (virtualPath.AppRelative, "~/App_Themes/")) {
				var themebp = new ThemeDirectoryBuildProvider ();
				themebp.SetVirtualPath (virtualPath);
				
				return GetSingleBuildProviderGroup (themebp);
			}

			CompilationSection section = CompilationSection;
			BuildProviderCollection bpcoll = section != null ? section.BuildProviders : null;

			if (bpcoll == null || bpcoll.Count == 0)
				return null;
			
			if (virtualPath.IsFake) {
				BuildProvider bp = GetBuildProvider (virtualPath, bpcoll);

				if (bp == null)
					return null;

				return GetSingleBuildProviderGroup (bp);
			}

			if (single) {
				AddVirtualFile (GetVirtualFile (virtualPath.Absolute), bpcoll);
			} else {
				var cache = new Dictionary <string, bool> (RuntimeHelpers.StringEqualityComparer);
				AddVirtualDir (GetVirtualDirectory (virtualPath.Absolute), bpcoll, cache);
				cache = null;
				if (buildProviders == null || buildProviders.Count == 0)
					AddVirtualFile (GetVirtualFile (virtualPath.Absolute), bpcoll);
			}

			if (buildProviders == null || buildProviders.Count == 0)
					return null;
			
			var buildProviderGroups = new List <BuildProviderGroup> ();
			foreach (BuildProvider bp in buildProviders.Values)
				AssignToGroup (bp, buildProviderGroups);

			if (buildProviderGroups == null || buildProviderGroups.Count == 0) {
				buildProviderGroups = null;
				return null;
			}
			
			// We need to reverse the order, so that the build happens from the least
			// dependant assemblies to the most dependant ones, more or less.
			buildProviderGroups.Reverse ();			
			
			return buildProviderGroups;
		}
		
		bool AddBuildProvider (BuildProvider buildProvider)
		{
			if (buildProviders == null)
				buildProviders = new Dictionary <string, BuildProvider> (RuntimeHelpers.StringEqualityComparer);
			
			string bpPath = buildProvider.VirtualPath;
			if (buildProviders.ContainsKey (bpPath))
				return false;

			buildProviders.Add (bpPath, buildProvider);
			return true;
		}
		
		void AddVirtualDir (VirtualDirectory vdir, BuildProviderCollection bpcoll, Dictionary <string, bool> cache)
		{
			if (vdir == null)
				return;
			
			BuildProvider bp;
			IDictionary <string, bool> deps;
			var dirs = new List <string> ();
			string fileVirtualPath;
			
			foreach (VirtualFile file in vdir.Files) {
				fileVirtualPath = file.VirtualPath;
				if (BuildManager.IgnoreVirtualPath (fileVirtualPath))
					continue;
				
				bp = GetBuildProvider (fileVirtualPath, bpcoll);
				if (bp == null)
					continue;
				if (!AddBuildProvider (bp))
					continue;
				
				deps = bp.ExtractDependencies ();
				if (deps == null)
					continue;

				string depDir, s;
				dirs.Clear ();
				foreach (var dep in deps) {
					s = dep.Key;
					depDir = VirtualPathUtility.GetDirectory (s); // dependencies are assumed to contain absolute paths
					if (cache.ContainsKey (depDir))
						continue;
					cache.Add (depDir, true);
					AddVirtualDir (GetVirtualDirectory (s), bpcoll, cache);
				}
			}
		}
		
		void AddVirtualFile (VirtualFile file, BuildProviderCollection bpcoll)
		{
			if (file == null || BuildManager.IgnoreVirtualPath (file.VirtualPath))
				return;
			
			BuildProvider bp = GetBuildProvider (file.VirtualPath, bpcoll);
			if (bp == null)
				return;
			AddBuildProvider (bp);
		}

		List <BuildProviderGroup> GetSingleBuildProviderGroup (BuildProvider bp)
		{
			var ret = new List <BuildProviderGroup> ();
			var group = new BuildProviderGroup ();
			group.AddProvider (bp);
			ret.Add (group);

			return ret;
		}
		
		VirtualDirectory GetVirtualDirectory (string virtualPath)
		{
			if (!vpp.DirectoryExists (VirtualPathUtility.GetDirectory (virtualPath)))
				return null;

			return vpp.GetDirectory (virtualPath);
		}

		VirtualFile GetVirtualFile (string virtualPath)
		{
			if (!vpp.FileExists (virtualPath))
				return null;
			
			return vpp.GetFile (virtualPath);
		}

		Type GetBuildProviderCodeDomType (BuildProvider bp)
		{
			CompilerType ct = bp.CodeCompilerType;
			if (ct == null) {
				string language = bp.LanguageName;

				if (String.IsNullOrEmpty (language))
					language = CompilationSection.DefaultLanguage;

				ct = BuildManager.GetDefaultCompilerTypeForLanguage (language, CompilationSection, false);
			}

			Type ret = ct != null ? ct.CodeDomProviderType : null;
			if (ret == null)
				throw new HttpException ("Unable to determine code compilation language provider for virtual path '" + bp.VirtualPath + "'.");

			return ret;
		}
		
		void AssignToGroup (BuildProvider buildProvider, List <BuildProviderGroup> groups)
		{
			if (IsDependencyCycle (buildProvider))
				throw new HttpException ("Dependency cycles are not suppported: " + buildProvider.VirtualPath);

			BuildProviderGroup myGroup = null;
			string bpVirtualPath = buildProvider.VirtualPath;
			string bpPath = VirtualPathUtility.GetDirectory (bpVirtualPath);
			bool canAdd;

			if (BuildManager.HasCachedItemNoLock (buildProvider.VirtualPath))
				return;			

			StringComparison stringComparison = RuntimeHelpers.StringComparison;
			if (buildProvider is ApplicationFileBuildProvider || buildProvider is ThemeDirectoryBuildProvider) {
				// global.asax and theme directory go into their own assemblies
				myGroup = new BuildProviderGroup ();
				myGroup.Standalone = true;
				InsertGroup (myGroup, groups);
			} else {
				Type bpCodeDomType = GetBuildProviderCodeDomType (buildProvider);
				foreach (BuildProviderGroup group in groups) {
					if (group.Standalone)
						continue;
					
					if (group.Count == 0) {
						myGroup = group;
						break;
					}

					canAdd = true;
					foreach (BuildProvider bp in group) {
						if (IsDependency (buildProvider, bp)) {
							canAdd = false;
							break;
						}
					
						// There should be one assembly per virtual dir
						if (String.Compare (bpPath, VirtualPathUtility.GetDirectory (bp.VirtualPath), stringComparison) != 0) {
							canAdd = false;
							break;
						}

						// Different languages go to different assemblies
						if (bpCodeDomType != null) {
							Type type = GetBuildProviderCodeDomType (bp);
							if (type != null) {
								if (type != bpCodeDomType) {
									canAdd = false;
									break;
								}
							}
						}
					}

					if (!canAdd)
						continue;

					myGroup = group;
					break;
				}
				
				if (myGroup == null) {
					myGroup = new BuildProviderGroup ();
					InsertGroup (myGroup, groups);
				}
			}
			
			myGroup.AddProvider (buildProvider);
			if (String.Compare (bpPath, virtualPathDirectory, stringComparison) == 0)
				myGroup.Master = true;
		}

		void InsertGroup (BuildProviderGroup group, List <BuildProviderGroup> groups)
		{
			if (group.Application) {
				groups.Insert (groups.Count - 1, group);
				return;
			}

			int index;
			if (group.Standalone)
				index = groups.FindLastIndex (SkipApplicationGroup);
			else
				index = groups.FindLastIndex (SkipStandaloneGroups);

			if (index == -1)
				groups.Add (group);
			else
				groups.Insert (index == 0 ? 0 : index - 1, group);
		}

		static bool SkipStandaloneGroups (BuildProviderGroup group)
		{
			if (group == null)
				return false;
			
			return group.Standalone;
		}

		static bool SkipApplicationGroup (BuildProviderGroup group)
		{
			if (group == null)
				return false;

			return group.Application;
		}
		
		bool IsDependency (BuildProvider bp1, BuildProvider bp2)
		{
			IDictionary <string, bool> deps = bp1.ExtractDependencies ();
			if (deps == null)
				return false;

			if (deps.ContainsKey (bp2.VirtualPath))
				return true;

			BuildProvider bp;
			// It won't loop forever as by the time this method is called, we are sure there are no cycles
			foreach (var dep in deps) {
				if (!buildProviders.TryGetValue (dep.Key, out bp))
					continue;

				if (IsDependency (bp, bp2))
					return true;
			}
			
			return false;
		}
		
		bool IsDependencyCycle (BuildProvider buildProvider)
		{
			var cache = new Dictionary <BuildProvider, bool> ();
			cache.Add (buildProvider, true);
			return IsDependencyCycle (cache, buildProvider.ExtractDependencies ());
		}

		bool IsDependencyCycle (Dictionary <BuildProvider, bool> cache, IDictionary <string, bool> deps)
		{
			if (deps == null)
				return false;

			BuildProvider bp;
			foreach (var d in deps) {
				if (!buildProviders.TryGetValue (d.Key, out bp))
					continue;
				if (cache.ContainsKey (bp))
					return true;
				cache.Add (bp, true);
				if (IsDependencyCycle (cache, bp.ExtractDependencies ()))
					return true;
				cache.Remove (bp);
			}
			
			return false;
		}

		public static BuildProvider GetBuildProvider (string virtualPath, BuildProviderCollection coll)
		{
			return GetBuildProvider (new VirtualPath (virtualPath), coll);
		}
		
		public static BuildProvider GetBuildProvider (VirtualPath virtualPath, BuildProviderCollection coll)
		{
			if (virtualPath == null || String.IsNullOrEmpty (virtualPath.Original) || coll == null)
				return null;
			
			string extension = virtualPath.Extension;
			BuildProvider bp = coll.GetProviderInstanceForExtension (extension);
			if (bp == null) {
				if (String.Compare (extension, ".asax", StringComparison.OrdinalIgnoreCase) == 0)
					bp = new ApplicationFileBuildProvider ();
				else if (StrUtils.StartsWith (virtualPath.AppRelative, "~/App_Themes/"))
					bp = new ThemeDirectoryBuildProvider ();

				if (bp != null)
					bp.SetVirtualPath (virtualPath);
				
				return bp;
			}
			
			object[] attrs = bp.GetType ().GetCustomAttributes (typeof (BuildProviderAppliesToAttribute), true);
			if (attrs == null || attrs.Length == 0)
				return bp;

			BuildProviderAppliesTo appliesTo = ((BuildProviderAppliesToAttribute)attrs [0]).AppliesTo;
			if ((appliesTo & BuildProviderAppliesTo.Web) == 0)
				return null;

			bp.SetVirtualPath (virtualPath);
			return bp;
		}
	}
}

