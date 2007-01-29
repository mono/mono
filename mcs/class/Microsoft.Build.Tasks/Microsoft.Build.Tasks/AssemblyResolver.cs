//
// AssemblyResolver.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	internal class AssemblyResolver {

		// name -> (version -> assemblypath)
		Dictionary <string, Dictionary <Version, string>> gac;

		Dictionary <string, Dictionary <Version, string>> hint_path_assemblies;
		Dictionary <string, object> hint_paths;
	
		public AssemblyResolver ()
		{
			gac = new Dictionary <string, Dictionary <Version, string>> ();
			hint_path_assemblies = new Dictionary <string, Dictionary <Version, string>> ();
			GatherGacAssemblies ();
		}

		string GetGacPath ()
		{
			// NOTE: code from mcs/tools/gacutil/driver.cs
			PropertyInfo gac = typeof (System.Environment).GetProperty ("GacPath", BindingFlags.Static | BindingFlags.NonPublic);

			if (gac == null)
				return null;

			MethodInfo get_gac = gac.GetGetMethod (true);
			return (string) get_gac.Invoke (null, null);
		}

		void GatherGacAssemblies ()
		{
			string gac_path = GetGacPath ();
			if (gac_path == null)
				throw new InvalidOperationException ("XBuild must be run on Mono runtime");

			Version version;
			DirectoryInfo version_info, assembly_info;

			foreach (string assembly_name in Directory.GetDirectories (gac_path)) {
				assembly_info = new DirectoryInfo (assembly_name);
				foreach (string version_token in Directory.GetDirectories (assembly_name)) {
					foreach (string file in Directory.GetFiles (version_token, "*.dll")) {
						version_info = new DirectoryInfo (version_token);
						version = new Version (version_info.Name.Split (
							new char [] {'_'}, StringSplitOptions.RemoveEmptyEntries) [0]);

						if (!gac.ContainsKey (assembly_info.Name))
							gac.Add (assembly_info.Name, new Dictionary <Version, string> ());
						gac [assembly_info.Name].Add (version, file);
					}
				}
			}
		}

		void GatherHintPathAssemblies (string hintPath)
		{
			if (hint_paths.ContainsKey (hintPath))
				return;

			Assembly a;
			AssemblyName name;

			foreach (string assembly_name in Directory.GetDirectories (hintPath)) {
				try {
					a = Assembly.ReflectionOnlyLoadFrom (assembly_name);
					name = new AssemblyName (a.FullName);

					if (!hint_path_assemblies.ContainsKey (name.Name))
						hint_path_assemblies [name.Name] = new Dictionary <Version, string> ();
					hint_path_assemblies [name.Name] [name.Version] = assembly_name;
				} catch {
				}
			}
		}

		public string ResolveAssemblyReference (ITaskItem reference)
		{
			AssemblyName name = null;

			try {
				name = new AssemblyName (reference.ItemSpec);
			} catch {
				return null;
			}

			return ResolveHintPathReference (name, reference.GetMetadata ("HintPath")) ?? ResolveGacReference (name);
		}

		string ResolveGacReference (AssemblyName name)
		{
			if (!gac.ContainsKey (name.Name))
				return null;

			if (name.Version != null) {
				if (!gac [name.Name].ContainsKey (name.Version))
					return null;
				else
					return gac [name.Name] [name.Version];
			}

			Version [] versions = new Version [gac [name.Name].Keys.Count];
			gac [name.Name].Keys.CopyTo (versions, 0);
			Array.Sort (versions, (IComparer <Version>) null);
			Version highest = versions [versions.Length - 1];
			
			return gac [name.Name] [highest];
		}

		string ResolveHintPathReference (AssemblyName name, string hintpath)
		{
			if (hintpath != String.Empty)
				GatherHintPathAssemblies (hintpath);

			if (!hint_path_assemblies.ContainsKey (name.Name))
				return null;

			if (name.Version != null) {
				if (!hint_path_assemblies [name.Name].ContainsKey (name.Version))
					return null;
				else
					return hint_path_assemblies [name.Name] [name.Version];
			}

			Version [] versions = new Version [hint_path_assemblies [name.Name].Keys.Count];
			hint_path_assemblies [name.Name].Keys.CopyTo (versions, 0);
			Array.Sort (versions, (IComparer <Version>) null);
			Version highest = versions [versions.Length - 1];
			
			return hint_path_assemblies [name.Name] [highest];
		}
	}
}

#endif
