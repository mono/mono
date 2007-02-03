//
// System.Web.Compilation.AspComponentFoundry
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.Collections;
using System.IO;
using System.Reflection;

#if NET_2_0
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
#endif

namespace System.Web.Compilation
{
	internal class AspComponentFoundry
	{
		private Hashtable foundries;

		public AspComponentFoundry ()
		{
#if NET_2_0
			foundries = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
#else
			foundries = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
						   CaseInsensitiveComparer.DefaultInvariant);
#endif

			Assembly sw = typeof (AspComponentFoundry).Assembly;
			RegisterFoundry ("asp", sw, "System.Web.UI.WebControls");
			RegisterFoundry ("", "object", typeof (System.Web.UI.ObjectTag));

#if NET_2_0
			RegisterConfigControls ();
#endif
		}

		public Type GetComponentType (string foundryName, string tag)
		{
			Foundry foundry = foundries [foundryName] as Foundry;
			if (foundry == null)
				return null;

			return foundry.GetType (tag);
		}

		public void RegisterFoundry (string foundryName,
						Assembly assembly,
						string nameSpace)
		{
			AssemblyFoundry foundry = new AssemblyFoundry (assembly, nameSpace);
			InternalRegister (foundryName, foundry);
		}

		public void RegisterFoundry (string foundryName,
						string tagName,
						Type type)
		{
			TagNameFoundry foundry = new TagNameFoundry (tagName, type);
			InternalRegister (foundryName, foundry);
		}

#if NET_2_0
		public void RegisterFoundry (string foundryName,
					     string tagName,
					     string source)
		{
			TagNameFoundry foundry = new TagNameFoundry (tagName, source);
			InternalRegister (foundryName, foundry);
		}
		
		// Look up the controls/namespaces defined in the config
		// file(s), resolve the assemblies but do not compile the types.
		void RegisterConfigControls ()
		{
			PagesSection pages = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
                        if (pages == null)
                                return;

                        TagPrefixCollection controls = pages.Controls;
                        if (controls == null || controls.Count == 0)
                                return;

			Dictionary <string, Assembly> assemblyCache = new Dictionary <string, Assembly> ();
                        foreach (TagPrefixInfo tpi in controls) {
                                if (!String.IsNullOrEmpty (tpi.TagName))
                                        RegisterFoundry (tpi.TagPrefix, tpi.TagName, tpi.Source);
                                else if (!String.IsNullOrEmpty (tpi.Namespace))
					RegisterFoundry (tpi.TagPrefix, GetAssemblyByName (assemblyCache, tpi.Assembly), tpi.Namespace);
                        }
		}

		Assembly GetAssemblyByName (Dictionary <string, Assembly> cache, string name)
		{
			if (cache.ContainsKey (name))
				return cache [name];

			Assembly assembly = null;
			Exception error = null;
			if (name.IndexOf (',') != -1) {
				try {
					assembly = Assembly.Load (name);
				} catch (Exception e) { error = e; }
			}

			if (assembly == null) {
				try {
					assembly = Assembly.LoadWithPartialName (name);
				} catch (Exception e) { error = e; }
			}
			
			if (assembly == null)
				throw new HttpException ("Assembly " + name + " not found", error);

			return assembly;
		}
#endif
		
		void InternalRegister (string foundryName, Foundry foundry)
		{
			object f = foundries [foundryName];
			if (f is CompoundFoundry) {
				((CompoundFoundry) f).Add (foundry);
			} else if (f == null || (f is AssemblyFoundry && foundry is AssemblyFoundry)) {
				// If more than 1 namespace/assembly specified, the last one is used.
				foundries [foundryName] = foundry;
			} else if (f != null) {
				CompoundFoundry compound = new CompoundFoundry (foundryName);
				compound.Add ((Foundry) f);
				compound.Add (foundry);
				foundries [foundryName] = compound;
			}
		}

		public bool LookupFoundry (string foundryName)
		{
			return foundries.Contains (foundryName);
		}

		abstract class Foundry
		{
			public abstract Type GetType (string componentName);
		}
		

		class TagNameFoundry : Foundry
		{
			string tagName;
			Type type;

#if NET_2_0
			string source;

			public TagNameFoundry (string tagName, string source)
			{
				this.tagName = tagName;
				this.source = source;
			}
#endif
			
			public TagNameFoundry (string tagName, Type type)
			{
				this.tagName = tagName;
				this.type = type;
			}

			public override Type GetType (string componentName)
			{
				if (0 != String.Compare (componentName, tagName, true))
					return null;

				return LoadType ();
			}

			Type LoadType ()
			{
#if NET_2_0
				if (type != null)
					return type;

				HttpContext context = HttpContext.Current;
				string vpath;
				string realpath;
				
				if (VirtualPathUtility.IsAppRelative (source)) {
					vpath = source;
					realpath = context.Request.MapPath (source);
				} else {
					vpath = VirtualPathUtility.ToAppRelative (source);
					realpath = source;
				}
				
				if ((type = CachingCompiler.GetTypeFromCache (realpath)) != null)
					return type;
				
				ArrayList other_deps = new ArrayList ();
                                type = UserControlParser.GetCompiledType (vpath, realpath, other_deps, context);
				if (type != null) {
					AspGenerator.AddTypeToCache (other_deps, realpath, type);
					WebConfigurationManager.ExtraAssemblies.Add (type.Assembly.Location);
				}
				return type;
#else
				return type;
#endif
			}
			
			public string TagName {
				get { return tagName; }
			}
		}

		class AssemblyFoundry : Foundry
		{
			string nameSpace;
			Assembly assembly;

			public AssemblyFoundry (Assembly assembly, string nameSpace)
			{
				this.assembly = assembly;
				this.nameSpace = nameSpace;
			}

			public override Type GetType (string componentName)
			{
				return assembly.GetType (nameSpace + "." + componentName, true, true);
			}
		}

		class CompoundFoundry : Foundry
		{
			AssemblyFoundry assemblyFoundry;
			Hashtable tagnames;
			string tagPrefix;

			public CompoundFoundry (string tagPrefix)
			{
				this.tagPrefix = tagPrefix;
#if NET_2_0
				tagnames = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
#else
				tagnames = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
							  CaseInsensitiveComparer.DefaultInvariant);
#endif
			}

			public void Add (Foundry foundry)
			{
				if (foundry is AssemblyFoundry) {
					assemblyFoundry = (AssemblyFoundry) foundry;
					return;
				}
				
				TagNameFoundry tn = (TagNameFoundry) foundry;
				string tagName = tn.TagName;
				if (tagnames.Contains (tagName)) {
					string msg = String.Format ("{0}:{1} already registered.", tagPrefix, tagName);
					throw new ApplicationException (msg);
				}
				tagnames.Add (tagName, foundry);
			}

			public override Type GetType (string componentName)
			{
				Type type = null;
				Foundry foundry = tagnames [componentName] as Foundry;
				if (foundry != null)
					return foundry.GetType (componentName);

				if (assemblyFoundry != null) {
					try {
						type = assemblyFoundry.GetType (componentName);
						return type;
					} catch { }
				}

				string msg = String.Format ("Type {0} not registered for prefix {1}",
							    componentName, tagPrefix);
				throw new ApplicationException (msg);
			}
		}
	}
}

