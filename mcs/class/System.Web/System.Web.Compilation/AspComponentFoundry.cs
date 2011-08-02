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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Compilation
{
	class AspComponentFoundry
	{
		Hashtable foundries;
		Dictionary <string, AspComponent> components;
		Dictionary <string, AspComponent> Components {
			get {
				if (components == null)
					components = new Dictionary <string, AspComponent> (StringComparer.OrdinalIgnoreCase);
				return components;
			}
		}
		
		public AspComponentFoundry ()
		{
			foundries = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
			Assembly sw = typeof (AspComponentFoundry).Assembly;
			RegisterFoundry ("asp", sw, "System.Web.UI.WebControls");
			RegisterFoundry ("", "object", typeof (System.Web.UI.ObjectTag));
			RegisterConfigControls ();
		}

		public AspComponent GetComponent (string tagName)
		{
			if (tagName == null || tagName.Length == 0)
				return null;
			
			if (components != null) {
				AspComponent ret;
				if (components.TryGetValue (tagName, out ret))
					return ret;
			}

			string foundryName, tag;
			int colon = tagName.IndexOf (':');
			if (colon > -1) {
				if (colon == 0)
					throw new Exception ("Empty TagPrefix is not valid.");
				if (colon + 1 == tagName.Length)
					return null;
				foundryName = tagName.Substring (0, colon);
				tag = tagName.Substring (colon + 1);
			} else {
				foundryName = String.Empty;
				tag = tagName;
			}
			
			object o = foundries [foundryName];			
			if (o == null)
				return null;

			Foundry foundry = o as Foundry;
			if (foundry != null)
				return CreateComponent (foundry, tagName, foundryName, tag);
			
			ArrayList af = o as ArrayList;
			if (af == null)
				return null;

			AspComponent component = null;
			Exception e = null;
			foreach (Foundry f in af) {
				try {
					component = CreateComponent (f, tagName, foundryName, tag);
					if (component != null)
						return component;
				} catch (Exception ex) {
					e = ex;
				}
			}

			if (e != null)
				throw e;
			
			return null;
		}

		AspComponent CreateComponent (Foundry foundry, string tagName, string prefix, string tag)
		{
			string source, ns;
			Type type;

			type = foundry.GetType (tag, out source, out ns);
			if (type == null)
				return null;
			
			AspComponent ret = new AspComponent (type, ns, prefix, source, foundry.FromConfig);
			Dictionary <string, AspComponent> components = Components;
			components.Add (tagName, ret);
			return ret;
		}
		
		public void RegisterFoundry (string foundryName, Assembly assembly, string nameSpace)
		{
			RegisterFoundry (foundryName, assembly, nameSpace, false);
		}
		
		public void RegisterFoundry (string foundryName,
					     Assembly assembly,
					     string nameSpace,
					     bool fromConfig)
		{
			AssemblyFoundry foundry = new AssemblyFoundry (assembly, nameSpace);
			foundry.FromConfig = fromConfig;
			InternalRegister (foundryName, foundry, fromConfig);
		}

		public void RegisterFoundry (string foundryName, string tagName, Type type)
		{
			RegisterFoundry (foundryName, tagName, type, false);
		}
		
		public void RegisterFoundry (string foundryName,
					     string tagName,
					     Type type,
					     bool fromConfig)
		{
			TagNameFoundry foundry = new TagNameFoundry (tagName, type);
			foundry.FromConfig = fromConfig;
			InternalRegister (foundryName, foundry, fromConfig);
		}

		public void RegisterFoundry (string foundryName, string tagName, string source)
		{
			RegisterFoundry (foundryName, tagName, source, false);
		}
		
		public void RegisterFoundry (string foundryName,
					     string tagName,
					     string source,
					     bool fromConfig)
		{
			TagNameFoundry foundry = new TagNameFoundry (tagName, source);
			foundry.FromConfig = fromConfig;
			InternalRegister (foundryName, foundry, fromConfig);
		}

		public void RegisterAssemblyFoundry (string foundryName,
						     string assemblyName,
						     string nameSpace,
						     bool fromConfig)
		{
			AssemblyFoundry foundry = new AssemblyFoundry (assemblyName, nameSpace);
			foundry.FromConfig = fromConfig;
			InternalRegister (foundryName, foundry, fromConfig);
		}		

		void RegisterConfigControls ()
		{
			PagesSection pages = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
			if (pages == null)
				return;

			TagPrefixCollection controls = pages.Controls;
			if (controls == null || controls.Count == 0)
				return;
			
			IList appCode = BuildManager.CodeAssemblies;
			bool haveCodeAssemblies = appCode != null && appCode.Count > 0;
			Assembly asm;
			foreach (TagPrefixInfo tpi in controls) {
				if (!String.IsNullOrEmpty (tpi.TagName))
					RegisterFoundry (tpi.TagPrefix, tpi.TagName, tpi.Source, true);
				else if (String.IsNullOrEmpty (tpi.Assembly)) {
					if (haveCodeAssemblies) {
						foreach (object o in appCode) {
							asm = o as Assembly;
							if (asm == null)
								continue;
							RegisterFoundry (tpi.TagPrefix, asm, tpi.Namespace, true);
						}
					}
				} else if (!String.IsNullOrEmpty (tpi.Namespace))
					RegisterAssemblyFoundry (tpi.TagPrefix,
								 tpi.Assembly,
								 tpi.Namespace,
								 true);
			}
		}
		
		void InternalRegister (string foundryName, Foundry foundry, bool fromConfig)
		{
			object f = foundries [foundryName];
			Foundry newFoundry = null;
			
			if (f is CompoundFoundry) {
				((CompoundFoundry) f).Add (foundry);
				return;
			} else if (f == null || f is ArrayList || (f is AssemblyFoundry && foundry is AssemblyFoundry)) {
				newFoundry = foundry;
			} else if (f != null) {
				CompoundFoundry compound = new CompoundFoundry (foundryName);
				compound.Add ((Foundry) f);
				compound.Add (foundry);
				newFoundry = foundry;
				newFoundry.FromConfig = fromConfig;
			}

			if (newFoundry == null)
				return;

			if (f == null) {
				foundries [foundryName] = newFoundry;
				return;
			}

			ArrayList af = f as ArrayList;
			if (af == null) {
				af = new ArrayList (2);
				af.Add (f);
				foundries [foundryName] = af;
			}

			if (newFoundry is AssemblyFoundry) {
				object o;
				for (int i = 0; i < af.Count; i++) {
					o = af [i];
					if (o is AssemblyFoundry) {
						af.Insert (i, newFoundry);
						return;
					}
				}
				af.Add (newFoundry);
			} else
				af.Insert (0, newFoundry);
		}

		public bool LookupFoundry (string foundryName)
		{
			return foundries.Contains (foundryName);
		}

		abstract class Foundry
		{
			bool _fromConfig;

			public bool FromConfig {
				get { return _fromConfig; }
				set { _fromConfig = value; }
			}
			
			public abstract Type GetType (string componentName, out string source, out string ns);
		}
		

		class TagNameFoundry : Foundry
		{
			string tagName;
			Type type;
			string source;

			public bool FromWebConfig {
				get { return source != null; }
			}
			
			public TagNameFoundry (string tagName, string source)
			{
				this.tagName = tagName;
				this.source = source;
			}
			
			public TagNameFoundry (string tagName, Type type)
			{
				this.tagName = tagName;
				this.type = type;
			}

			public override Type GetType (string componentName, out string source, out string ns)
			{
				source = null;
				ns = null;
				if (0 != String.Compare (componentName, tagName, true, Helpers.InvariantCulture))
					return null;

				source = this.source;
				return LoadType ();
			}

			Type LoadType ()
			{
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

				type = BuildManager.GetCompiledType (vpath);
				if (type != null) {
					AspGenerator.AddTypeToCache (null, realpath, type);
					BuildManager.AddToReferencedAssemblies (type.Assembly);
				}
				return type;
			}
			
			public string TagName {
				get { return tagName; }
			}
		}

		class AssemblyFoundry : Foundry
		{
			string nameSpace;
			Assembly assembly;
			string assemblyName;
			Dictionary <string, Assembly> assemblyCache;
			
			public AssemblyFoundry (Assembly assembly, string nameSpace)
			{
				this.assembly = assembly;
				this.nameSpace = nameSpace;

				if (assembly != null)
					this.assemblyName = assembly.FullName;
				else
					this.assemblyName = null;
			}

			public AssemblyFoundry (string assemblyName, string nameSpace)
			{
				this.assembly = null;
				this.nameSpace = nameSpace;
				this.assemblyName = assemblyName;
			}
			
			public override Type GetType (string componentName, out string source, out string ns)
			{
				source = null;
				ns = nameSpace;

				if (assembly == null && assemblyName != null)
					assembly = GetAssemblyByName (assemblyName, true);

				string typeName = String.Concat (nameSpace, ".", componentName);
				if (assembly != null)
					return assembly.GetType (typeName, false, true);

				IList tla = BuildManager.TopLevelAssemblies;
				if (tla != null && tla.Count > 0) {
					Type ret = null;
					foreach (Assembly asm in tla) {
						if (asm == null)
							continue;
						ret = asm.GetType (typeName, false, true);
						if (ret != null)
							return ret;
					}
				}

				return null;
			}

			Assembly GetAssemblyByName (string name, bool throwOnMissing)
			{
				if (assemblyCache == null)
					assemblyCache = new Dictionary <string, Assembly> ();
				
				if (assemblyCache.ContainsKey (name))
					return assemblyCache [name];
				
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
					if (throwOnMissing)
						throw new HttpException ("Assembly " + name + " not found", error);
					else
						return null;

				assemblyCache.Add (name, assembly);
				return assembly;
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
				tagnames = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
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
					if (tn.FromWebConfig)
						return;

					string msg = String.Format ("{0}:{1} already registered.", tagPrefix, tagName);
					throw new ApplicationException (msg);
				}
				tagnames.Add (tagName, foundry);
			}

			public override Type GetType (string componentName, out string source, out string ns)
			{
				source = null;
				ns = null;
				Type type = null;
				Foundry foundry = tagnames [componentName] as Foundry;
				if (foundry != null)
					return foundry.GetType (componentName, out source, out ns);

				if (assemblyFoundry != null) {
					try {
						type = assemblyFoundry.GetType (componentName, out source, out ns);
						return type;
					} catch { }
				}

				string msg = String.Format ("Type {0} not registered for prefix {1}", componentName, tagPrefix);
				throw new ApplicationException (msg);
			}
		}
	}
}
